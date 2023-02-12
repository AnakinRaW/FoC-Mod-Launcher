using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocLauncher.Update.Utilities;

internal static class UpdateUtilities
{
    private const int KiloByteThreshold = 1024;
    private const int MegaByteThreshold = KiloByteThreshold  * 1024;
    private const long GigaByteThreshold = MegaByteThreshold * 1024;

    internal static string ToHumanReadableSize(long sizeInBytes)
    {
        string unit;
        double sizeInUnit;
        if (sizeInBytes >= GigaByteThreshold)
        {
            sizeInUnit = (double) sizeInBytes / GigaByteThreshold;
            unit = "GB";
        }
        else if (sizeInBytes >= MegaByteThreshold)
        {
            sizeInUnit = (double)sizeInBytes / GigaByteThreshold;
            unit = "MB";
        }
        else if (sizeInBytes >= KiloByteThreshold)
        {
            sizeInUnit = (double)sizeInBytes / GigaByteThreshold;
            unit = "KB";
        }
        else
        {
            sizeInUnit = (double)sizeInBytes / GigaByteThreshold;
            unit = "B";
        }

        var approximatedSize = Math.Round(Math.Ceiling(sizeInUnit * 100.0) / 100.0, 2);
        var f = FormattableString.Invariant($"{approximatedSize:#,##0.##} {unit}");
        return f.ToString(CultureInfo.CurrentCulture);
    }
}