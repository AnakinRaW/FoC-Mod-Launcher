using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Theming;
using AnakinRaW.ProductMetadata.Services;
using AnakinRaW.ProductUpdater;
using AnakinRaW.ProductUpdater.Services;
using FocLauncher.Services;
using FocLauncher.Update.ProductMetadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Serilog.Extensions.Logging;
using Sklavenwalker.CommonUtilities.DownloadManager;
using Sklavenwalker.CommonUtilities.DownloadManager.Configuration;
using Sklavenwalker.CommonUtilities.DownloadManager.Verification;
using Sklavenwalker.CommonUtilities.FileSystem;
using Sklavenwalker.CommonUtilities.FileSystem.Windows;
using Sklavenwalker.CommonUtilities.Registry;
using Sklavenwalker.CommonUtilities.Registry.Windows;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace FocLauncher;

internal static class Program
{
    private static ServiceCollection _serviceCollection = null!;
    private static IServiceProvider _coreServices = null!;

    [STAThread]
    private static int Main(string[] args)
    {
        Task.Run(() => LauncherAssemblyInfo.AssemblyName);
        
        _serviceCollection = CreateCoreServices();
        _coreServices = _serviceCollection.BuildServiceProvider();

        // Since logging directory is not yet assured, we cannot run under the global exception handler.
        new AppRestoreHandler(_coreServices).RestoreIfNecessary();
        
        // TODO: Parse args
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
        // TODO: Log parsed CMD
        var exitCode = ExecuteInternal(logger);
        logger.LogTrace($"Exit Code: {exitCode}");
        return exitCode;
    }

    private static int ExecuteInternal(ILogger logger)
    {
        BuildLauncherServices(_serviceCollection);

        var launcherServices = _serviceCollection.BuildServiceProvider();
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

        CreateUpdateProgramServices(serviceCollection);

        _serviceCollection.MakeReadOnly();
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

        var environment = new LauncherEnvironment(fileSystem);
        serviceCollection.AddSingleton<ILauncherEnvironment>(environment);

        SetLogging(serviceCollection, fileSystem, environment);
        serviceCollection.AddTransient<IRegistry>(_ => new WindowsRegistry());
        serviceCollection.AddSingleton<ILauncherRegistry>(sp => new LauncherRegistry(sp));

        return serviceCollection;
    }

    private static void CreateUpdateProgramServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddProductUpdater();
        
        serviceCollection.AddSingleton<IProductService>(sp => new LauncherProductService(sp));
        serviceCollection.AddSingleton<IProductUpdateProviderService>(sp => new ProductUpdateProviderService(sp));
        serviceCollection.AddSingleton<IBranchManager>(sp => new LauncherBranchManager(sp));
        serviceCollection.AddSingleton<IManifestLoader>(sp => new LauncherManifestLoader(sp));
        serviceCollection.AddSingleton<IDownloadManager>(sp => new DownloadManager(sp));
        serviceCollection.AddSingleton<IVerificationManager>(sp => new VerificationManager(sp));
        serviceCollection.AddSingleton(CreateDownloadConfiguration());
    }

    private static IDownloadManagerConfiguration CreateDownloadConfiguration()
    {
        return new DownloadManagerConfiguration { VerificationPolicy = VerificationPolicy.Optional };
    }

    [SuppressMessage("ReSharper", "RedundantAssignment")]
    private static void SetLogging(IServiceCollection serviceCollection, IFileSystem fileSystem, ILauncherEnvironment environment)
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