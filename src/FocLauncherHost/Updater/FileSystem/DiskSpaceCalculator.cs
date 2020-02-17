using System;
using System.Collections.Generic;
using FocLauncherHost.Updater.Component;

namespace FocLauncherHost.Updater.FileSystem
{
    internal class DiskSpaceCalculator
    {
        public IDictionary<string, DriveSpaceData> CalculatedDiskSizes { get; }

        public bool HasEnoughDiskSpace { get; } = true;

        public DiskSpaceCalculator(IComponent component, long additionalBuffer = 0)
        {
            CalculatedDiskSizes = new Dictionary<string, DriveSpaceData>(StringComparer.OrdinalIgnoreCase);

            var destinationRoot = FileSystemExtensions.GetPathRoot(component.Destination);
            var backupRoot = FileSystemExtensions.GetPathRoot(UpdateConfiguration.Instance.BackupPath);

            if (string.IsNullOrEmpty(backupRoot)) 
                backupRoot = destinationRoot;

            SetSizeMembers(component.OriginInfo?.Size, destinationRoot);
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
    }
}