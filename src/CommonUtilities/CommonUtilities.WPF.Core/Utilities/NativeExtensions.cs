using System;
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

    internal static int GetXLParam(int lParam)
    {
        return LoWord(lParam);
    }

    internal static int GetYLParam(int lParam)
    {
        return HiWord(lParam);
    }

    internal static int HiWord(int value)
    {
        return (short)(value >> 16);
    }

    internal static int HiWord(long value)
    {
        return (short)(value >> 16 & ushort.MaxValue);
    }

    internal static int LoWord(int value)
    {
        return (short)(value & ushort.MaxValue);
    }

    internal static int LoWord(long value)
    {
        return (short)(value & ushort.MaxValue);
    }

    internal static int ToInt32Unchecked(this IntPtr value)
    {
        return (int)value.ToInt64();
    }

    internal static IntPtr MakeParam(int lowWord, int highWord)
    {
        return new(lowWord & ushort.MaxValue | highWord << 16);
    }

    public static Int32Rect ToInt32Rect(this RECT rect)
    {
        return new(rect.Left, rect.Top, rect.Width, rect.Height);
    }
}