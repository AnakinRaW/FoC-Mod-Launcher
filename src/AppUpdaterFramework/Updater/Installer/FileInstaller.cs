using System;
using AnakinRaW.AppUpaterFramework.Updater.Progress;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Updater.Installer;

internal class FileInstaller : IInstaller
{
    public event EventHandler<ProgressEventArgs>? Progress;

    private readonly IServiceProvider _serviceProvider;

    public FileInstaller(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
    }
}