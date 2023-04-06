using System;
using System.IO.Abstractions;
using System.Reflection;
using AnakinRaW.ApplicationBase;
using AnakinRaW.ApplicationBase.Utilities;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using FocLauncher.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Serilog.Extensions.Logging;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.FileSystem.Windows;
using AnakinRaW.CommonUtilities.DownloadManager.Verification.HashVerification;
using AnakinRaW.CommonUtilities.DownloadManager.Verification;
using AnakinRaW.CommonUtilities.DownloadManager;
using AnakinRaW.CommonUtilities.DownloadManager.Configuration;
using AnakinRaW.CommonUtilities.Windows;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;
using AnakinRaW.ExternalUpdater;
using FocLauncher.Imaging;

namespace FocLauncher;

public abstract class ProgramBase
{
    private static ServiceCollection _serviceCollection = null!;
    private static IServiceProvider _coreServices = null!;

    protected int Run(string[] args)
    {
        _serviceCollection = CreateCoreServices();
        _coreServices = _serviceCollection.BuildServiceProvider();

        if (args.Length >= 1)
        {
            var updaterResult = ExternalUpdaterResult.UpdaterNotRun;
            var argument = args[0];
            if (int.TryParse(argument, out var value) && Enum.IsDefined(typeof(ExternalUpdaterResult), value))
                updaterResult = (ExternalUpdaterResult)value;
            var registry = _coreServices.GetRequiredService<IApplicationUpdaterRegistry>();
            new ExternalUpdaterResultHandler(registry).Handle(updaterResult);
        }

        // Since logging directory is not yet assured, we cannot run under the global exception handler.
        new AppResetHandler(_coreServices).ResetIfNecessary();
        
        using (new UnhandledExceptionHandler(_coreServices))
            return Execute(args);
    }

    protected abstract ServiceCollection CreateCoreServices();

    protected abstract int Run(string[] args, IServiceProvider services);

    protected virtual void BuildApplicationServices(IServiceCollection serviceCollection)
    {
    }

    private int Execute(string[] args)
    {
        var logger = _coreServices.GetService<ILoggerFactory>()?.CreateLogger(typeof(Program));
        var env = _coreServices.GetRequiredService<IApplicationEnvironment>();
        logger?.LogTrace($"Application Version: {env.AssemblyInfo.InformationalVersion}");
        logger?.LogTrace($"Raw Command line: {Environment.CommandLine}");
        var exitCode = ExecuteInternal(logger);
        logger?.LogTrace($"Exit Code: {exitCode}");
        return exitCode;
    }

    private int ExecuteInternal(ILogger? logger)
    {
        BuildApplicationServices();

        var appServices = _serviceCollection.BuildServiceProvider();

        var updateRegistry = appServices.GetRequiredService<IApplicationUpdaterRegistry>();
        if (updateRegistry.RequiresUpdate)
        {
            logger?.LogInformation("Update required: Running external updater...");
            try
            {
                appServices.GetRequiredService<IRegistryExternalUpdaterLauncher>().Launch();
                logger?.LogInformation("External updater running. Closing application!");
                return 0;
            }
            catch (Exception e)
            {
                logger?.LogError(e, $"Failed to run update. Starting main application normally: {e.Message}");
                updateRegistry.Clear();
            }
        }

        var exitCode = new LauncherApplication(appServices).Run();
        logger?.LogTrace($"Closing application with exit code {exitCode}");
        return exitCode;
    }

    private void BuildApplicationServices()
    {
        BuildApplicationServices(_serviceCollection);
        _serviceCollection.MakeReadOnly();
    }
}

internal class Program : ProgramBase
{
    [STAThread]
    private static int Main(string[] args)
    {
        return new Program().Run(args);
    }

    protected override ServiceCollection CreateCoreServices()
    {
        var serviceCollection = new ServiceCollection();

        var fileSystem = new FileSystem();
        var windowsPathService = new WindowsPathService(fileSystem);
        var windowsFileSystemService = new WindowsFileSystemService(fileSystem);
        serviceCollection.AddSingleton<IFileSystem>(fileSystem);
        serviceCollection.AddSingleton<IFileSystemService>(windowsFileSystemService);
        serviceCollection.AddSingleton(windowsFileSystemService);
        serviceCollection.AddSingleton<IWindowsPathService>(windowsPathService);
        serviceCollection.AddSingleton<IPathHelperService>(new PathHelperService(fileSystem));

        var environment = new LauncherEnvironment(Assembly.GetExecutingAssembly(), fileSystem);
        serviceCollection.AddSingleton<IApplicationEnvironment>(environment);

        serviceCollection.AddSingleton<IResourceExtractor>(sp =>
            new CosturaResourceExtractor(environment.AssemblyInfo.CurrentAssembly, sp));


        SetLogging(serviceCollection, fileSystem, environment);

        serviceCollection.AddTransient<IRegistry>(_ => new WindowsRegistry());

        serviceCollection.AddSingleton<ILauncherRegistry>(sp => new LauncherRegistry(sp));
        serviceCollection.AddSingleton<IApplicationUpdaterRegistry>(sp => new ApplicationUpdaterRegistry(LauncherRegistry.LauncherRegistryPath, sp));

        return serviceCollection;
    }

    protected override int Run(string[] args, IServiceProvider services)
    {
        throw new NotImplementedException();
    }

    protected override void BuildApplicationServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDownloadManager>(sp => new DownloadManager(sp));
        serviceCollection.AddSingleton<IVerificationManager>(sp =>
        {
            var vm = new VerificationManager(sp);
            vm.RegisterVerifier("*", new HashVerifier(sp));
            return vm;
        });

        serviceCollection.AddSingleton(CreateDownloadConfiguration());

        serviceCollection.AddTransient<IStatusBarFactory>(_ => new LauncherStatusBarFactory()); 
        serviceCollection.AddTransient(_ => ConnectionManager.Instance);

        serviceCollection.AddApplicationFramework();
        serviceCollection.AddApplicationBase(ImageKeys.AppIcon);
    }

    private static IDownloadManagerConfiguration CreateDownloadConfiguration()
    {
        return new DownloadManagerConfiguration { VerificationPolicy = VerificationPolicy.Optional };
    }

    private static void SetLogging(IServiceCollection serviceCollection, IFileSystem fileSystem, IApplicationEnvironment environment)
    {
        serviceCollection.AddLogging(l =>
        {
#if DEBUG
            l.AddDebug();
#endif
            l.AddConsole();
            SetFileLogging(l);
        }).Configure<LoggerFilterOptions>(o =>
        {
#if DEBUG
            o.AddFilter<DebugLoggerProvider>(null, LogLevel.Trace);
#endif
            o.AddFilter<SerilogLoggerProvider>(null, LogLevel.Trace);
        });


        void SetFileLogging(ILoggingBuilder builder)
        {
            var logPath = fileSystem.Path.Combine(
                fileSystem.Path.GetTempPath(), LauncherEnvironment.LauncherLogDirectoryName, "launcher.log");
            var fileLogLevel = LogLevel.Information;
            var version = environment.AssemblyInfo.InformationalAsSemVer();
            if (version is not null && version.IsPrerelease)
                fileLogLevel = LogLevel.Debug;
#if DEBUG
            logPath = "log.txt";
            fileLogLevel = LogLevel.Trace;
#endif
            builder.AddFile(logPath, fileLogLevel);
        }
    }
}