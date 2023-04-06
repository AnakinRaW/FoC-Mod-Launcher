using System;
using System.IO.Abstractions;
using AnakinRaW.ApplicationBase.Utilities;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.FileSystem.Windows;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using AnakinRaW.ExternalUpdater;
using AnakinRaW.ExternalUpdater.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AnakinRaW.ApplicationBase;

public abstract class ProgramBase
{
    private readonly ServiceCollection _serviceCollection = new();


    protected abstract IApplicationEnvironment CreateEnvironment(IServiceProvider serviceProvider);


    protected virtual void CreateCoreServicesBeforeEnvironment(IServiceCollection serviceCollection) { }

    protected virtual void CreateCoreServicesAfterEnvironment(IServiceCollection serviceCollection) { }
    
    protected int Run(string[] args)
    {
        CreateCoreServices();
        var coreServices = _serviceCollection.BuildServiceProvider();

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
        new AppResetHandler(coreServices).ResetIfNecessary();

        using (new UnhandledExceptionHandler(coreServices))
            return Execute(args, coreServices);
    }


    private int Execute(string[] args, IServiceProvider coreServices)
    {
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

        var exitCode = ExecuteInternal(args, _serviceCollection);
        logger?.LogTrace($"Exit Code: {exitCode}");
        return exitCode;
    }

    protected abstract int ExecuteInternal(string[] args, IServiceCollection serviceCollection);

    private void CreateCoreServices()
    {
        var fileSystem = new FileSystem();
        var windowsFileSystemService = new WindowsFileSystemService(fileSystem);
        _serviceCollection.AddSingleton<IFileSystem>(fileSystem);
        _serviceCollection.AddSingleton<IFileSystemService>(windowsFileSystemService);
        _serviceCollection.AddSingleton(windowsFileSystemService);
        _serviceCollection.AddSingleton<IWindowsPathService>(_ => new WindowsPathService(fileSystem));
        _serviceCollection.AddTransient<IRegistry>(_ => new WindowsRegistry());

        CreateCoreServicesBeforeEnvironment(_serviceCollection);

        using var environmentServiceProvider = _serviceCollection.BuildServiceProvider();
        var environment = CreateEnvironment(environmentServiceProvider);
        _serviceCollection.AddSingleton(environment);
        
        CreateCoreServicesAfterEnvironment(_serviceCollection);

        _serviceCollection.TryAddSingleton<IApplicationUpdaterRegistry>(sp => new ApplicationUpdaterRegistry(environment.ApplicationRegistryPath, sp));
        _serviceCollection.TryAddSingleton<IExternalUpdaterLauncher>(sp => new ExternalUpdaterLauncher(sp));
        _serviceCollection.TryAddSingleton<IRegistryExternalUpdaterLauncher>(sp => new RegistryExternalUpdaterLauncher(sp));

        _serviceCollection.TryAddSingleton<IResourceExtractor>(sp =>
            new CosturaResourceExtractor(environment.AssemblyInfo.CurrentAssembly, sp));
    }
}