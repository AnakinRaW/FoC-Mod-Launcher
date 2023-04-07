using System;
using System.IO.Abstractions;
using AnakinRaW.ApplicationBase.Update;
using AnakinRaW.ApplicationBase.Utilities;
using AnakinRaW.AppUpdaterFramework.Configuration;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.AppUpdaterFramework.Product.Manifest;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.FileSystem.Windows;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using AnakinRaW.ExternalUpdater;
using AnakinRaW.ExternalUpdater.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using AnakinRaW.ApplicationBase.Services;
using AnakinRaW.AppUpdaterFramework;

namespace AnakinRaW.ApplicationBase;

public abstract class BootstrapperBase
{
    protected abstract IApplicationEnvironment CreateEnvironment(IServiceProvider serviceProvider);


    protected virtual void CreateCoreServicesBeforeEnvironment(IServiceCollection serviceCollection) { }

    protected virtual void CreateCoreServicesAfterEnvironment(IServiceCollection serviceCollection) { }
    
    protected int Run(string[] args)
    {
        var serviceCollection = CreateCoreServices();
        var coreServices = serviceCollection.BuildServiceProvider();

        if (args.Length >= 1)
        {
            var updaterResult = ExternalUpdaterResult.UpdaterNotRun;
            var argument = args[0];
            if (int.TryParse(argument, out var value) && Enum.IsDefined(typeof(ExternalUpdaterResult), value))
                updaterResult = (ExternalUpdaterResult)value;
            var registry = coreServices.GetRequiredService<IApplicationUpdaterRegistry>();
            new ExternalUpdaterResultHandler(registry).Handle(updaterResult);
        }

        // Since logging directory is not yet assured, we cannot run under the global exception handler.
        var resetHandler = coreServices.GetRequiredService<IAppResetHandler>();
        resetHandler.ResetIfNecessary();

        using (coreServices.GetRequiredService<IUnhandledExceptionHandler>())
        {
            return ExecuteInternal(args, serviceCollection);
        }
    }


    private int ExecuteInternal(string[] args, IServiceCollection serviceCollection)
    {
        var coreServices = serviceCollection.BuildServiceProvider();
        var logger = coreServices.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        var env = coreServices.GetRequiredService<IApplicationEnvironment>();
        var updateRegistry = coreServices.GetRequiredService<IApplicationUpdaterRegistry>();


        logger?.LogTrace($"Application Version: {env.AssemblyInfo.InformationalVersion}");
        logger?.LogTrace($"Raw Command line: {Environment.CommandLine}");

        if (updateRegistry.RequiresUpdate)
        {
            logger?.LogInformation("Update required: Running external updater...");
            try
            {
                coreServices.GetRequiredService<IRegistryExternalUpdaterLauncher>().Launch();
                logger?.LogInformation("External updater running. Closing application!");
                return 0;
            }
            catch (Exception e)
            {
                logger?.LogError(e, $"Failed to run update. Starting main application normally: {e.Message}");
                updateRegistry.Clear();
            }
        }

        CreateApplicationServices(serviceCollection);

        var exitCode = Execute(args, serviceCollection);
        logger?.LogTrace($"Exit Code: {exitCode}");
        return exitCode;
    }

    private protected virtual void CreateApplicationServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IProductService>(sp => new ApplicationProductService(sp));
        serviceCollection.AddSingleton<IBranchManager>(sp => new ApplicationBranchManager(sp));
        serviceCollection.AddSingleton<IUpdateConfigurationProvider>(sp => new ApplicationUpdateConfigurationProvider(sp));
        serviceCollection.AddSingleton<IInstalledManifestProvider>(sp => new ApplicationInstalledManifestProvider(sp));
        serviceCollection.AddSingleton<IManifestLoader>(sp => new JsonManifestLoader(sp));
    }

    protected abstract int Execute(string[] args, IServiceCollection serviceCollection);

    private IServiceCollection CreateCoreServices()
    {
        var serviceCollection = new ServiceCollection();
        
        var fileSystem = new FileSystem();
        var windowsFileSystemService = new WindowsFileSystemService(fileSystem);
        serviceCollection.AddSingleton<IFileSystem>(fileSystem);
        serviceCollection.AddSingleton<IFileSystemService>(windowsFileSystemService);
        serviceCollection.AddSingleton(windowsFileSystemService);
        serviceCollection.AddSingleton<IWindowsPathService>(_ => new WindowsPathService(fileSystem));
        serviceCollection.AddTransient<IRegistry>(_ => new WindowsRegistry());

        CreateCoreServicesBeforeEnvironment(serviceCollection);

        using var environmentServiceProvider = serviceCollection.BuildServiceProvider();
        var environment = CreateEnvironment(environmentServiceProvider);
        serviceCollection.AddSingleton(environment);
        
        CreateCoreServicesAfterEnvironment(serviceCollection);

        serviceCollection.TryAddSingleton<IApplicationUpdaterRegistry>(sp => new ApplicationUpdaterRegistry(environment.ApplicationRegistryPath, sp));
        serviceCollection.TryAddSingleton<IExternalUpdaterLauncher>(sp => new ExternalUpdaterLauncher(sp));
        serviceCollection.TryAddSingleton<IRegistryExternalUpdaterLauncher>(sp => new RegistryExternalUpdaterLauncher(sp));

        serviceCollection.TryAddSingleton<IResourceExtractor>(sp =>
            new CosturaResourceExtractor(environment.AssemblyInfo.CurrentAssembly, sp));

        serviceCollection.TryAddSingleton<IAppResetHandler>(sp => new AppResetHandler(sp));
        serviceCollection.TryAddTransient<IUnhandledExceptionHandler>(sp => new UnhandledExceptionHandler(sp));

        return serviceCollection;
    }
}