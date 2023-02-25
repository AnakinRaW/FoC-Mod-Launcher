using System;
using System.Collections.Generic;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Updater.Installer;

internal class InstallerFactory : IInstallerFactory
{
    private readonly Dictionary<ComponentType, IInstaller> _installers = new();
    private readonly IServiceProvider _serviceProvider;

    public InstallerFactory(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
    }

    public IInstaller CreateInstaller(IInstallableComponent component)
    {
        Requires.NotNull(component, nameof(component));
        switch (component.Type)
        {
            case ComponentType.File:
                if (!_installers.ContainsKey(component.Type))
                    _installers[component.Type] = new FileInstaller(_serviceProvider);
                return _installers[component.Type];
            default:
                throw new NotSupportedException($"Component type '{component.Type}' is not supported.");
        }
    }
}