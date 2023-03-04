using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Updater;
using AnakinRaW.AppUpdaterFramework.Updater.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Utilities;

internal class DiskSpaceCalculator : IDiskSpaceCalculator 
{
    internal const long AdditionalSizeBuffer = 20_000_000;

    private readonly IFileSystem _fileSystem;
    private readonly IUpdateConfiguration _updateConfiguration;

    public DiskSpaceCalculator(IServiceProvider serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _updateConfiguration = serviceProvider.GetRequiredService<IUpdateConfigurationProvider>().GetConfiguration();
    }

    public void ThrowIfNotEnoughDiskSpaceAvailable(
        IInstallableComponent newComponent, 
        IInstallableComponent? oldComponent, 
        string? installPath, 
        CalculationOptions options)
    {
        Requires.NotNull(newComponent, nameof(newComponent));
        foreach (var diskData in GetDiskInformation(newComponent, oldComponent, installPath, options))
        {
            if (!diskData.HasEnoughDiskSpace)
                throw new OutOfDiskspaceException(
                    $"There is not enough space to install “{newComponent.GetDisplayName()}”. {diskData.RequestedSize + AdditionalSizeBuffer} is required on drive {diskData.DriveName}  " +
                    $"but you only have {diskData.AvailableDiskSpace} available.");
        }
    }

    private IEnumerable<DriveSpaceData> GetDiskInformation(IInstallableComponent newComponent, IInstallableComponent? oldComponent, string? installPath, CalculationOptions options)
    {
        var calculatedDiskSizes = new Dictionary<string, DriveSpaceData>();


        if (options.HasFlag(CalculationOptions.Download))
        {
            var downloadRoot = _fileSystem.Path.GetPathRoot(_updateConfiguration.TempDownloadLocation);
            if (!string.IsNullOrEmpty(downloadRoot))
                UpdateSizeInformation(newComponent.OriginInfo?.Size, downloadRoot!);
        }

        if (options.HasFlag(CalculationOptions.Install))
        {
            var destinationRoot = _fileSystem.Path.GetPathRoot(installPath);
            if (!string.IsNullOrEmpty(destinationRoot))
                UpdateSizeInformation(newComponent.OriginInfo?.Size, destinationRoot!);
        }
        
        if (options.HasFlag(CalculationOptions.Backup) && oldComponent != null)
        {
            var backupRoot = _fileSystem.Path.GetPathRoot(_updateConfiguration.BackupLocation);
            if (!string.IsNullOrEmpty(backupRoot)) 
                UpdateSizeInformation(oldComponent.InstallationSize.Total, backupRoot!);
        }

        foreach (var sizes in calculatedDiskSizes)
        {
            try
            {
                var freeSpace = _fileSystem.DriveInfo.New(sizes.Key).AvailableFreeSpace;
                sizes.Value.AvailableDiskSpace = freeSpace;
                sizes.Value.HasEnoughDiskSpace = freeSpace >= sizes.Value.RequestedSize + AdditionalSizeBuffer;
            }
            catch
            {
                sizes.Value.HasEnoughDiskSpace = false;
            }
        }

        return calculatedDiskSizes.Values;


        void UpdateSizeInformation(long? actualSize, string drive) {
            if (!actualSize.HasValue)
                return;
            if (!calculatedDiskSizes.ContainsKey(drive))
                calculatedDiskSizes.Add(drive, new DriveSpaceData(actualSize.Value, drive));
            else
                calculatedDiskSizes[drive].RequestedSize += actualSize.Value;
        }
    }

    [Flags]
    public enum CalculationOptions
    {
        Install = 1,
        Download = 2,
        Backup = 4,
        All = Install | Download | Backup
    }
}