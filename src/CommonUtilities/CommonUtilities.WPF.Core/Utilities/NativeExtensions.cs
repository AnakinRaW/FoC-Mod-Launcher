using System.Windows;
using Vanara.PInvoke;

namespace AnakinRaW.CommonUtilities.Wpf.Utilities;

internal static class NativeExtensions
{
    internal static bool IsPrimary(this User32.MONITORINFO monitorinfo)
    {
        return monitorinfo.dwFlags == User32.MonitorInfoFlags.MONITORINFOF_PRIMARY;
    }
    
    internal static RECT AsRECT(this Int32Rect rect)
    {
        return new(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
    }

    internal static RECT ToRECT(this Rect rect)
    {
        return new RECT((int) rect.Left, (int)rect.Top, (int) rect.Right, (int) rect.Bottom);
    }

    internal static Rect ToWpfRect(this RECT rect)
    {
        return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
    } 

    internal static Point GetPosition(this RECT rect)
    {
        return new Point(rect.left, rect.top);
    }

    internal static Size ToSize(this RECT rect)
    {
        return new Size(rect.Width, rect.Height);
    }

    internal static POINT ToPOINT(this Point point)
    {
        return new()
        {
            X = (int)point.X,
            Y = (int)point.Y
        };
    }
}