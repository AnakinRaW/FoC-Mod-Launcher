using System;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Updater.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace FocLauncher.Update.LauncherImplementations;

internal class LauncherUpdateConfigurationProvider : UpdateConfigurationProviderBase
{
    private readonly ILauncherEnvironment _launcherEnv;
    private readonly IFileSystem _fileSystem;

    public LauncherUpdateConfigurationProvider(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _launcherEnv = serviceProvider.GetRequiredService<ILauncherEnvironment>();
    }

    protected override IUpdateConfiguration CreateConfiguration()
    {
        var downloadLocation = _fileSystem.Path.Combine(_launcherEnv.ApplicationLocalPath, "downloads");
        var backupsLocation = _fileSystem.Path.Combine(_launcherEnv.ApplicationLocalPath, "backups");
        return new UpdateConfiguration
        {
            DownloadRetryCount = 3,
            TempDownloadLocation = downloadLocation,
            BackupLocation = backupsLocation,
            BackupPolicy = BackupPolicy.Required
        };
    }
}