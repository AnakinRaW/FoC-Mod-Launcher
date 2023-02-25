using System;

namespace AnakinRaW.AppUpaterFramework.Utilities;

public class DriveSpaceData : IEquatable<DriveSpaceData>
{
    public long RequestedSize { get; set; }

    public bool HasEnoughDiskSpace { get; set; }

    public long AvailableDiskSpace { get; set; }

    public string DriveName { get; }


    public DriveSpaceData(long currentInstallSize, string driveName)
    {
        RequestedSize = currentInstallSize;
        DriveName = driveName;
    }

    public bool Equals(DriveSpaceData? other)
    {
        return string.Equals(DriveName, other?.DriveName, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return DriveName.ToLowerInvariant().GetHashCode();
    }
}