using System;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;
using AnakinRaW.ExternalUpdater.CLI;
using AnakinRaW.ExternalUpdater.CLI.Arguments;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.AppUpdaterFramework.ExternalUpdater;

internal class RegistryExternalUpdaterLauncher : IRegistryExternalUpdaterLauncher
{
    private readonly IApplicationUpdaterRegistry _registry;
    private readonly IExternalUpdaterLauncher _launcher;
    private readonly IFileSystem _fileSystem;

    public RegistryExternalUpdaterLauncher(IServiceProvider serviceProvider)
    {
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
        if (string.IsNullOrEmpty(args))
            throw new NotSupportedException("No updater arguments set.");

        var launchArgs = ExternalUpdaterArgumentUtilities.FromString(args!).WithCurrentData();
        _launcher.Start(updater, launchArgs);
    }
}