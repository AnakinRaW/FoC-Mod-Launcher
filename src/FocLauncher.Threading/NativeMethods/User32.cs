using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace FocLauncher.NativeMethods
{
    internal static class User32
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        internal static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IntersectRect(out RECT lprcDst, ref RECT lprcSrc1, ref RECT lprcSrc2);

        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromPoint(POINT pt, int flags);

        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromRect([In] ref RECT rect, int flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo monitorInfo);

        [DllImport("user32.dll")]
        internal static extern uint GetDpiForWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref POINT point);

        internal static Point GetCursorPos()
        {
            var point1 = new POINT { X = 0, Y = 0 };
            var point2 = new Point();
            if (GetCursorPos(ref point1))
            {
                point2.X = point1.X;
                point2.Y = point1.Y;
            }
            return point2;
        }
    }
}