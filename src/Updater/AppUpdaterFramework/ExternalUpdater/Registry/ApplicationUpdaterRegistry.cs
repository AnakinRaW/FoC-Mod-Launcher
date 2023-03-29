using System;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.ExternalUpdater.Options;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;

public sealed class ApplicationUpdaterRegistry : IApplicationUpdaterRegistry
{
    private readonly IRegistryKey _registryKey;

    public bool Reset
    {
        get
        {
            _registryKey.GetValueOrDefault(nameof(Reset), out var value, false);
            return value;
        }
        private set => _registryKey.SetValue(nameof(Reset), value);
    }

    public bool RequiresUpdate
    {
        get
        {
            _registryKey.GetValueOrDefault(nameof(RequiresUpdate), out var value, false);
            return value;
        }
        private set => _registryKey.SetValue(nameof(RequiresUpdate), value);
    }

    public string? UpdateCommandArgs
    {
        get
        {
            _registryKey.GetValueOrDefault(nameof(UpdateCommandArgs), out var value, (string)null!);
            return value;
        }
        private set
        {
            if (string.IsNullOrEmpty(value))
                _registryKey.DeleteValue(nameof(UpdateCommandArgs));
            else
                _registryKey.SetValue(nameof(UpdateCommandArgs), value!);
        }
    }

    public string? UpdaterPath
    {
        get
        {
            _registryKey.GetValueOrDefault(nameof(UpdaterPath), out var value, (string)null!);
            return value;
        }
        private set
        {
            if (string.IsNullOrEmpty(value))
                _registryKey.DeleteValue(nameof(UpdaterPath));
            else
                _registryKey.SetValue(nameof(UpdaterPath), value!);
        }
    }

    public ApplicationUpdaterRegistry(string basePath, IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        var registry = serviceProvider.GetRequiredService<IRegistry>();
        var baseKey = registry.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
        var registryKey = baseKey.CreateSubKey(basePath);
        _registryKey = registryKey ?? throw new InvalidOperationException("Unable to create Launcher registry. Missing rights?");
    }

    public void ScheduleReset()
    {
        Reset = true;
    }

    public void Clear()
    {
        _registryKey.DeleteValue(nameof(Reset));
        _registryKey.DeleteValue(nameof(RequiresUpdate));
        _registryKey.DeleteValue(nameof(UpdateCommandArgs));
        _registryKey.DeleteValue(nameof(UpdaterPath));
    }

    public void ScheduleUpdate(IFileInfo updater, ExternalUpdaterOptions options)
    {
        Requires.NotNull(updater, nameof(updater));
        Requires.NotNull(options, nameof(options));
        RequiresUpdate = true;
        UpdaterPath = updater.FullName;
        UpdateCommandArgs = options.ToArgs();
    }
}