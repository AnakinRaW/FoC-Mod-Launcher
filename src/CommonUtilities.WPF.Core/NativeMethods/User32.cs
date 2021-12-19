using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.NativeMethods;

internal static class User32
{
    [DllImport("user32.dll")]
    internal static extern IntPtr GetThreadDpiAwarenessContext();

    [DllImport("user32.dll")]
    internal static extern bool AreDpiAwarenessContextsEqual(IntPtr dpiContextA, IntPtr dpiContextB);

    [DllImport("user32.dll")]
    internal static extern IntPtr SetThreadDpiAwarenessContext(IntPtr awareness);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoStruct monitorInfo);

    [DllImport("user32.dll")]
    internal static extern uint GetDpiForWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IntersectRect(out RectStruct lprcDst, [In] ref RectStruct lprcSrc1, [In] ref RectStruct lprcSrc2);

    [DllImport("user32.dll")]
    internal static extern IntPtr MonitorFromRect([In] ref RectStruct rect, int flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr CreateWindowEx(int dwExStyle, string className, string lpWindowName, int dwStyle,
        int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(ref PointStruct pointStruct);

    internal static Point GetCursorPos()
    {
        var point = new PointStruct { x = 0, y = 0 };
        var cursorPos = new Point();
        if (GetCursorPos(ref point))
        {
            cursorPos.X = point.x;
            cursorPos.Y = point.y;
        }
        return cursorPos;
    }

    [DllImport("user32.dll")]
    internal static extern IntPtr MonitorFromPoint(PointStruct pt, int flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int nMsg, IntPtr wParam, IntPtr lParam);
}