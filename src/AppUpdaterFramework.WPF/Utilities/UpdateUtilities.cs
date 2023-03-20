using System;
using System.Globalization;

namespace AnakinRaW.AppUpdaterFramework.Utilities;

internal static class UpdateUtilities
{
    private const int KiloByteThreshold = 1024;
    private const int MegaByteThreshold = KiloByteThreshold  * 1024;
    private const long GigaByteThreshold = MegaByteThreshold * 1024;

    internal static string ToHumanReadableSize(long sizeInBytes)
    {
        string unit;
        double sizeInUnit;
        var useRounding = false;
        if (sizeInBytes >= GigaByteThreshold)
        {
            sizeInUnit = (double) sizeInBytes / GigaByteThreshold;
            unit = "GB";
            useRounding = true;
        }
        else if (sizeInBytes >= MegaByteThreshold)
        {
            sizeInUnit = (double)sizeInBytes / MegaByteThreshold;
            unit = "MB";
        }
        else if (sizeInBytes >= KiloByteThreshold)
        {
            sizeInUnit = (double)sizeInBytes / KiloByteThreshold;
            unit = "KB";
        }
        else
        {
            sizeInUnit = sizeInBytes;
            unit = "B";
        }

        var approximatedSize = useRounding ? Math.Round(Math.Ceiling(sizeInUnit * 100.0) / 100.0, 2) : Math.Ceiling(sizeInUnit);
        var f = FormattableString.Invariant($"{approximatedSize:#,##0.##} {unit}");
        return f.ToString(CultureInfo.CurrentCulture);
    }
}