using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework;
using AnakinRaW.AppUpdaterFramework.Configuration;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.AppUpdaterFramework.Product.Manifest;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Theming;
using FocLauncher.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Serilog.Extensions.Logging;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.FileSystem.Windows;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using FocLauncher.Utilities;
using FocLauncher.Commands.Handlers;
using AnakinRaW.CommonUtilities.DownloadManager.Verification.HashVerification;
using AnakinRaW.CommonUtilities.DownloadManager.Verification;
using AnakinRaW.CommonUtilities.DownloadManager;
using FocLauncher.Update.LauncherImplementations;
using AnakinRaW.CommonUtilities.DownloadManager.Configuration;
using AnakinRaW.CommonUtilities.Windows;
using ImageKeys = FocLauncher.Imaging.ImageKeys;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;
using AnakinRaW.ExternalUpdater;
using AnakinRaW.ExternalUpdater.Services;
using FocLauncher.Update.Manifest;

namespace FocLauncher;

internal static class Program
{
    private static ServiceCollection _serviceCollection = null!;
    private static IServiceProvider _coreServices = null!;

    [STAThread]
    private static int Main(string[] args)
    {
        Task.Run(() => LauncherAssemblyInfo.ExecutableFileName);
        
        _serviceCollection = CreateCoreServices();
        _coreServices = _serviceCollection.BuildServiceProvider();

        if (args.Length >= 1)
        {
            ExternalUpdaterResult updaterResult = ExternalUpdaterResult.UpdaterNotRun;
            var argument = args[0];
            if (int.TryParse(argument, out var value) && Enum.IsDefined(typeof(ExternalUpdaterResult), value))
                updaterResult = (ExternalUpdaterResult)value;
            var registry = _coreServices.GetRequiredService<IApplicationUpdaterRegistry>();
            new ExternalUpdaterResultHandler(registry).Handle(updaterResult);
        }

        // Since logging directory is not yet assured, we cannot run under the global exception handler.
        new AppResetHandler(_coreServices).ResetIfNecessary();
        
        int exitCode;
        using (GetUnhandledExceptionHandler()) 
            exitCode = Execute();
        return exitCode;
    }

    internal static int Execute()
    {
        var logger = _coreServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Program));
        logger.LogTrace($"FoC Mod Launcher Version: {LauncherAssemblyInfo.InformationalVersion}");
        logger.LogTrace($"Raw Command line: {Environment.CommandLine}");
        var exitCode = ExecuteInternal(logger);
        logger.LogTrace($"Exit Code: {exitCode}");
        return exitCode;
    }

    private static int ExecuteInternal(ILogger logger)
    {
        BuildLauncherServices(_serviceCollection);

        var launcherServices = _serviceCollection.BuildServiceProvider();

        var updateRegistry = launcherServices.GetRequiredService<IApplicationUpdaterRegistry>();


        if (updateRegistry.RequiresUpdate)
        {
            try
            {
                logger.LogInformation("Update required: Running external updater...");
                launcherServices.GetRequiredService<IRegistryExternalUpdaterLauncher>().Launch();
                logger.LogInformation("External updater running. Closing application!");
                return 0;
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failed to run update. Starting main application normally: {e.Message}");
                updateRegistry.Clear();
            }
        }

        var exitCode = new LauncherApplication(launcherServices).Run();
        logger.LogTrace($"Closing the launcher with exit code {exitCode}");
        return exitCode;
    }

    private static UnhandledExceptionHandler GetUnhandledExceptionHandler()
    {
        return new UnhandledExceptionHandler(_coreServices);
    }

    private static void BuildLauncherServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddApplicationFramework();

        serviceCollection.AddSingleton<IThemeManager>(sp => new ThemeManager(sp));
        serviceCollection.AddSingleton<IViewModelPresenter>(_ => new ViewModelPresenterService());

        serviceCollection.AddSingleton<IModalWindowFactory>(sp => new LauncherModalWindowFactory(sp));
        serviceCollection.AddSingleton<IDialogFactory>(sp => new LauncherDialogFactory(sp));
        serviceCollection.AddSingleton<IDialogButtonFactory>(_ => new DialogButtonFactory(true));

        serviceCollection.AddTransient<IStatusBarFactory>(_ => new LauncherStatusBarFactory()); 
        serviceCollection.AddTransient(_ => ConnectionManager.Instance); 

        CreateUpdateServices(serviceCollection);

        _serviceCollection.MakeReadOnly();
    }

    private static void CreateUpdateServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddUpdateGui(ImageKeys.AppIcon);

        serviceCollection.AddSingleton<IProductService>(sp => new LauncherProductService(sp));
        serviceCollection.AddSingleton<IBranchManager>(sp => new LauncherBranchManager(sp));
        serviceCollection.AddSingleton<IManifestLoader>(sp => new LauncherManifestLoader(sp));
        serviceCollection.AddSingleton<IUpdateConfigurationProvider>(sp => new LauncherUpdateConfigurationProvider(sp));
        serviceCollection.AddSingleton<IInstalledManifestProvider>(sp => new LauncherInstalledManifestProvider(sp));

        serviceCollection.AddSingleton<IDownloadManager>(sp => new DownloadManager(sp));
        serviceCollection.AddSingleton<IVerificationManager>(sp =>
        {
            var vm = new VerificationManager(sp);
            vm.RegisterVerifier("*", new HashVerifier(sp));
            return vm;
        });

        serviceCollection.AddSingleton(CreateDownloadConfiguration());

        serviceCollection.AddSingleton(sp => new LauncherUpdateInteractionFactory(sp));
        serviceCollection.AddSingleton<IUpdateDialogViewModelFactory>(sp => sp.GetRequiredService<LauncherUpdateInteractionFactory>());

        serviceCollection.AddSingleton<IExternalUpdaterLauncher>(sp => new ExternalUpdaterLauncher(sp));
    }

    private static IDownloadManagerConfiguration CreateDownloadConfiguration()
    {
        return new DownloadManagerConfiguration { VerificationPolicy = VerificationPolicy.Optional };
    }


    private static ServiceCollection CreateCoreServices()
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
        serviceCollection.AddSingleton<ICosturaResourceExtractor>(sp =>
            new ResourceExtractor(LauncherAssemblyInfo.CurrentAssembly, sp));

        var environment = new LauncherEnvironment(fileSystem);
        serviceCollection.AddSingleton<ILauncherEnvironment>(environment);

        SetLogging(serviceCollection, fileSystem);

        serviceCollection.AddTransient<IRegistry>(_ => new WindowsRegistry());
        serviceCollection.AddSingleton<ILauncherRegistry>(sp => new LauncherRegistry(sp));
        
        serviceCollection.AddSingleton<IApplicationUpdaterRegistry>(sp => new ApplicationUpdaterRegistry(LauncherRegistry.LauncherRegistryPath, sp));
        
        serviceCollection.AddSingleton<IShowUpdateWindowCommandHandler>(sp => new ShowUpdateWindowCommandHandler(sp));

        return serviceCollection;
    }

    private static void SetLogging(IServiceCollection serviceCollection, IFileSystem fileSystem)
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
                fileSystem.Path.GetTempPath(), 
                LauncherEnvironment.LauncherLogDirectoryName, 
                "launcher.log");
            var fileLogLevel = LogLevel.Information;
            var version = LauncherAssemblyInfo.InformationalAsSemVer();
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