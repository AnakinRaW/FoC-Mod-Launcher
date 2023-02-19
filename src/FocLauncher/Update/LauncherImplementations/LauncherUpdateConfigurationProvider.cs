using System;
using System.IO.Abstractions;
using AnakinRaW.AppUpaterFramework.Updater.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace FocLauncher.Update.LauncherImplementations;

internal class LauncherUpdateConfigurationProvider : UpdateConfigurationProviderBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILauncherEnvironment _launcherEnv;
    private readonly IFileSystem _fileSystem;

    public LauncherUpdateConfigurationProvider(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _launcherEnv = serviceProvider.GetRequiredService<ILauncherEnvironment>();
    }

    protected override IUpdateConfiguration CreateConfiguration()
    {
        var downloadLocation = _fileSystem.Path.Combine(_launcherEnv.ApplicationLocalPath, "downloads");
        return new UpdateConfiguration
        {
            DownloadRetryCount = 3,
            TempDownloadLocation = downloadLocation
        };
    }
}