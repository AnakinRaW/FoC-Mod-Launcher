using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Security.AccessControl;
using System.Threading.Tasks;
using FocLauncher.Controls;
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
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Services;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Services;
using Sklavenwalker.ProductMetadata.Services;
using Sklavenwalker.ProductUpdater.Services;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using UnhandledExceptionDialogViewModel = FocLauncher.ViewModels.UnhandledExceptionDialogViewModel;

namespace FocLauncher;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        Task.Run(() => LauncherAssemblyInfo.AssemblyName);
        var launcherServices = BuildLauncherServiceProvider();
        // TODO: Parse args
        int exitCode;
        using (GetUnhandledExceptionHandler(launcherServices)) 
            exitCode = Execute(launcherServices);
        return exitCode;
    }

    internal static int Execute(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Program));
        logger.LogTrace($"FoC Mod Launcher Version: {LauncherAssemblyInfo.InformationalVersion}");
        logger.LogTrace($"Raw Command line: {Environment.CommandLine}");
        // TODO: Log parsed CMD
        var exitCode = ExecuteInternal(services, logger);
        logger.LogTrace($"Exit Code: {exitCode}");
        return exitCode;
    }

    private static int ExecuteInternal(IServiceProvider services, ILogger logger)
    {
        new AppRestoreHandler(services).RestoreIfRequested();
        var exitCode = new LauncherApplication(services).Run();
        logger.LogTrace($"Closing the launcher with exit code {exitCode}");
        return exitCode;
    }

    private static UnhandledExceptionHandler GetUnhandledExceptionHandler(IServiceProvider launcherServices)
    {
        return new UnhandledExceptionHandler(launcherServices);
    }

    private static IServiceProvider BuildLauncherServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        CreateProgramServices(serviceCollection);
        
        serviceCollection.AddSingleton<IThemeManager>(sp => new ThemeManager());
        serviceCollection.AddSingleton<IViewModelPresenter>(_ => new ViewPresenterService());
        serviceCollection.AddSingleton<IWindowService>(sp => new WindowService());
        serviceCollection.AddSingleton<IModalWindowService>(sp => new ModalWindowService(sp));
        serviceCollection.AddSingleton<IModalWindowFactory>(sp => new ModalWindowFactory(sp));
        serviceCollection.AddSingleton<IQueuedDialogService>(sp => new QueuedDialogService(sp));
        serviceCollection.AddSingleton<IDialogFactory>(sp => new DialogFactory(sp));
        serviceCollection.AddSingleton<IDialogButtonFactory>(_ => new DialogButtonFactory(true));
        serviceCollection.AddSingleton<IThreadHelper>(_ => new ThreadHelper());

        serviceCollection.AddTransient<IStatusBarFactory>(_ => new LauncherStatusBarFactory()); 

        CreateUpdateProgramServices(serviceCollection);
        
        return serviceCollection.BuildServiceProvider();
    }
    
    private static void CreateProgramServices(IServiceCollection serviceCollection)
    {
        var fileSystem = new FileSystem();
        var windowsPathService = new WindowsPathService();
        var windowsFileSystemService = new WindowsFileSystemService(fileSystem);
        serviceCollection.AddSingleton<IFileSystem>(fileSystem);
        serviceCollection.AddSingleton<IFileSystemService>(windowsFileSystemService);
        serviceCollection.AddSingleton(windowsFileSystemService);
        serviceCollection.AddSingleton<IWindowsPathService>(windowsPathService);
        serviceCollection.AddSingleton<IPathHelperService>(new PathHelperService(fileSystem));
        var environment = new LauncherEnvironment(fileSystem);
        serviceCollection.AddSingleton<ILauncherEnvironment>(environment);
        InitializeApplicationBasePath(windowsPathService, environment);
        SetLogging(serviceCollection, fileSystem, environment);
        serviceCollection.AddTransient<IRegistry>(_ => new WindowsRegistry());
        serviceCollection.AddSingleton<ILauncherRegistry>(sp => new LauncherRegistry(sp));
    }

    private static void CreateUpdateProgramServices(IServiceCollection serviceCollection)
    {
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

    private static void InitializeApplicationBasePath(IWindowsPathService pathService, ILauncherEnvironment environment)
    {
        var appLocalPath = environment.ApplicationLocalDirectory;
        if (!pathService.UserHasDirectoryAccessRights(appLocalPath.Parent.FullName, FileSystemRights.CreateDirectories))
        {
            var exception = new IOException($"Permission on '{appLocalPath}' denied: Creating a new directory");
            new UnhandledExceptionDialog(new UnhandledExceptionDialogViewModel(exception)).ShowModal();
            throw exception;
        }

        appLocalPath.Create();
    }
}