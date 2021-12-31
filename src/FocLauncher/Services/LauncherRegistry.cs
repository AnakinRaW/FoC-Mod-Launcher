using System;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Registry;
using Validation;

namespace FocLauncher.Services;

internal sealed class LauncherRegistry : ILauncherRegistry
{
    internal const string LauncherRegistryPath = @"SOFTWARE\FocLauncher";

    private readonly IRegistryKey _registryKey;

    public bool Restore
    {
        get
        {
            _registryKey.GetValueOrDefault(nameof(Restore), out var value, false);
            return value;
        }
        set => _registryKey.SetValue(nameof(Restore), value);
    }

    public LauncherRegistry(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        var registry = serviceProvider.GetRequiredService<IRegistry>();
        var baseKey = registry.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
        var registryKey = baseKey.CreateSubKey(LauncherRegistryPath);
        _registryKey = registryKey ?? throw new InvalidOperationException("Unable to create Launcher registry. Missing rights?");
    }

    public void Reset()
    { 
        _registryKey.DeleteValue(nameof(Restore));
    }
}