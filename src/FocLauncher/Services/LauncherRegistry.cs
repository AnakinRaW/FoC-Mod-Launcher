using System;
using AnakinRaW.ApplicationBase;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace FocLauncher.Services;

internal sealed class LauncherRegistry : ILauncherRegistry
{
    private readonly IRegistryKey _registryKey;
    
    public LauncherRegistry(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        var registry = serviceProvider.GetRequiredService<IRegistry>();
        var env = serviceProvider.GetRequiredService<IApplicationEnvironment>();
        var baseKey = registry.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
        var registryKey = baseKey.CreateSubKey(env.ApplicationRegistryPath);
        _registryKey = registryKey ?? throw new InvalidOperationException("Unable to create Launcher registry. Missing rights?");
    }
}