using System;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;
using AnakinRaW.AppUpdaterFramework.Utilities;
using AnakinRaW.ExternalUpdater.Options;
using AnakinRaW.ExternalUpdater.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.AppUpdaterFramework.ExternalUpdater;

internal class RegistryExternalUpdaterLauncher : IRegistryExternalUpdaterLauncher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IApplicationUpdaterRegistry _registry;
    private readonly IExternalUpdaterLauncher _launcher;
    private readonly IFileSystem _fileSystem;

    public RegistryExternalUpdaterLauncher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _registry = serviceProvider.GetRequiredService<IApplicationUpdaterRegistry>();
        _launcher = serviceProvider.GetRequiredService<IExternalUpdaterLauncher>();
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    }

    public void Launch()
    {
        var updaterPath = _registry.UpdaterPath;
        if (string.IsNullOrEmpty(updaterPath))
            throw new NotSupportedException("No updater in registry set");
        var updater = _fileSystem.FileInfo.New(updaterPath!);

        var args = _registry.UpdateCommandArgs;
        if (args is null)
            throw new NotSupportedException("No updater options set.");

        var cpi = CurrentProcessInfo.Current;
        var launchOptions = ExternalUpdaterArgumentUtilities.FromArgs(args).WithCurrentData(cpi.ProcessFilePath, cpi.Id, _serviceProvider);
        _launcher.Start(updater, launchOptions);
    }
}