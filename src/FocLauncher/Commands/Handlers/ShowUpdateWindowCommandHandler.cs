using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework;
using AnakinRaW.AppUpaterFramework.Services;
using AnakinRaW.AppUpdaterFramework;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.AppUpdaterFramework.Product.Manifest;
using AnakinRaW.AppUpdaterFramework.Updater.Configuration;
using AnakinRaW.CommonUtilities.DownloadManager;
using AnakinRaW.CommonUtilities.DownloadManager.Configuration;
using AnakinRaW.CommonUtilities.DownloadManager.Verification;
using AnakinRaW.CommonUtilities.DownloadManager.Verification.HashVerification;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using FocLauncher.Update.Commands.Handlers;
using FocLauncher.Update.LauncherImplementations;
using FocLauncher.Update.ViewModels;
using FocLauncher.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace FocLauncher.Commands.Handlers;

internal class ShowUpdateWindowCommandHandler : AsyncCommandHandlerBase, IShowUpdateWindowCommandHandler
{
    private readonly IServiceProvider _parentServiceProvider;

    private readonly Lazy<IServiceProvider> _serviceProviderLazy;
    private readonly IConnectionManager _connectionManager;

    private IServiceProvider ServiceProvider => _serviceProviderLazy.Value;

    public ShowUpdateWindowCommandHandler(IServiceProvider serviceProvider)
    {
        _parentServiceProvider = serviceProvider;
        _serviceProviderLazy = new Lazy<IServiceProvider>(CreateUpdateServiceProvider);
        _connectionManager = serviceProvider.GetRequiredService<IConnectionManager>();
    }

    public override async Task HandleAsync()
    {
        await ExtractAssemblies();

        // Singletone instance of this view model drastically increases closing/cancellation complexity.
        // Creating a new model for each request should be good enough. 
        await using var serviceScope = ServiceProvider.CreateAsyncScope();
        var serviceProvider = serviceScope.ServiceProvider;

        var viewModel = new UpdateWindowViewModel(serviceProvider);
        await serviceProvider.GetRequiredService<IModalWindowService>().ShowModal(viewModel);
    }

    protected override bool CanHandle()
    {
        return _connectionManager.HasInternetConnection();
    }

    private async Task ExtractAssemblies()
    {
        var env = _parentServiceProvider.GetRequiredService<ILauncherEnvironment>();
        await _parentServiceProvider.GetRequiredService<ICosturaAssemblyExtractor>()
            .ExtractAssemblyAsync(LauncherConstants.AppUpdaterAssemblyName, env.ApplicationLocalPath);
    }

    private IServiceProvider CreateUpdateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();

        AddFromParentServiceProvider(serviceCollection);

        serviceCollection.AddUpdateFramework();

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
        serviceCollection.AddSingleton<IProductViewModelFactory>(sp => new ProductViewModelFactory(sp));
        serviceCollection.AddSingleton(CreateDownloadConfiguration());
        serviceCollection.AddSingleton<IUpdateCommandHandler>(sp => new UpdateCommandHandler(sp));

        serviceCollection.Replace(ServiceDescriptor.Scoped<IInteractionHandler>(sp => new LauncherInteractionHandler(sp)));

        return serviceCollection.BuildServiceProvider();
    }

    private static IDownloadManagerConfiguration CreateDownloadConfiguration()
    {
        return new DownloadManagerConfiguration { VerificationPolicy = VerificationPolicy.Optional };
    }

    private void AddFromParentServiceProvider(IServiceCollection serviceCollection)
    {
        var fileSystem = _parentServiceProvider.GetRequiredService<IFileSystem>();
        var fileSystemService = _parentServiceProvider.GetRequiredService<IFileSystemService>();
        var modalWindowService = _parentServiceProvider.GetRequiredService<IModalWindowService>();
        var launcherEnvironment = _parentServiceProvider.GetRequiredService<ILauncherEnvironment>();
        var dialogService = _parentServiceProvider.GetRequiredService<IQueuedDialogService>();
        var buttonFactory = _parentServiceProvider.GetRequiredService<IDialogButtonFactory>();

        SetLogging(serviceCollection, fileSystem, launcherEnvironment);

        serviceCollection.AddSingleton(_connectionManager);
        serviceCollection.AddSingleton(fileSystem);
        serviceCollection.AddSingleton(fileSystemService);
        serviceCollection.AddSingleton(modalWindowService);
        serviceCollection.AddSingleton(launcherEnvironment);
        serviceCollection.AddSingleton(dialogService);
        serviceCollection.AddSingleton(buttonFactory);
    }

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
                "launcher_update.log");
            var fileLogLevel = LogLevel.Information;
            var version = LauncherAssemblyInfo.InformationalAsSemVer();
            if (version is not null && version.IsPrerelease)
                fileLogLevel = LogLevel.Debug;
#if DEBUG
            logPath = "log_update.txt";
            fileLogLevel = LogLevel.Trace;
#endif
            builder.AddFile(logPath, fileLogLevel);
        }
    }
}