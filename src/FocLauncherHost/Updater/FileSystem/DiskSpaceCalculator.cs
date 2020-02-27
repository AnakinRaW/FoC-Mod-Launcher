using System;
using System.Collections.Generic;
using FocLauncherHost.Updater.Component;

namespace FocLauncherHost.Updater.FileSystem
{
    internal class DiskSpaceCalculator
    {
        public IDictionary<string, DriveSpaceData> CalculatedDiskSizes { get; }

        public bool HasEnoughDiskSpace { get; } = true;

        internal DiskSpaceCalculator(IComponent component, long additionalBuffer = 0, CalculationOption option = CalculationOption.All)
        {
            CalculatedDiskSizes = new Dictionary<string, DriveSpaceData>(StringComparer.OrdinalIgnoreCase);

            var destinationRoot = FileSystemExtensions.GetPathRoot(component.Destination);
            var backupRoot = FileSystemExtensions.GetPathRoot(UpdateConfiguration.Instance.BackupPath);

            if (string.IsNullOrEmpty(backupRoot)) 
                backupRoot = destinationRoot;

            

            if (ComponentDownloadPathStorage.Instance.TryGetValue(component, out var downloadPath) && option.HasFlag(CalculationOption.Download))
            {
                var downloadRoot = FileSystemExtensions.GetPathRoot(downloadPath);
                if (!string.IsNullOrEmpty(downloadPath))
                    SetSizeMembers(component.OriginInfo?.Size, downloadRoot);
            }

            if (option.HasFlag(CalculationOption.Install))
                SetSizeMembers(component.OriginInfo?.Size, destinationRoot);
            if (option.HasFlag(CalculationOption.Backup))
                SetSizeMembers(component.DiskSize, backupRoot);

            foreach (var sizes in CalculatedDiskSizes)
            {
                try
                {
                    var driveFreeSpace = FileSystemExtensions.GetDriveFreeSpace(sizes.Key);
                    sizes.Value.AvailableDiskSpace = driveFreeSpace;
                    sizes.Value.HasEnoughDiskSpace = driveFreeSpace >= sizes.Value.RequestedSize + additionalBuffer;
                }
                catch
                {
                    sizes.Value.HasEnoughDiskSpace = false;
                    HasEnoughDiskSpace = false;
                }
            }

        }

        public static void ThrowIfNotEnoughDiskSpaceAvailable(IComponent component, long additionalBuffer = 0,
            CalculationOption option = CalculationOption.All)
        {
            foreach (var diskData in new DiskSpaceCalculator(component, additionalBuffer, option).CalculatedDiskSizes)
            {
                if (!diskData.Value.HasEnoughDiskSpace)
                    throw new OutOfDiskspaceException(
                        $"There is not enough space to install “{component.Name}”. {diskData.Key} is required on drive {diskData.Value.RequestedSize + additionalBuffer} " +
                        $"but you only have {diskData.Value.AvailableDiskSpace} available.");
            }
        }

        private void SetSizeMembers(long? actualSize, string drive)
        {
            if (!actualSize.HasValue)
                return;
            if (!CalculatedDiskSizes.ContainsKey(drive))
                CalculatedDiskSizes.Add(drive, new DriveSpaceData(actualSize.Value, drive));
            else
            {
                CalculatedDiskSizes[drive].RequestedSize += actualSize.Value;
            }
        }

        [Flags]
        public enum CalculationOption
        {
            Install = 1,
            Download = 2,
            Backup = 4,
            All = Install | Download | Backup
        }
    }
}