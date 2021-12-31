using System;
using System.Collections.Generic;
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

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int SetWindowLong(IntPtr hWnd, short nIndex, int value);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
    public static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    public static int GetWindowLong(IntPtr hWnd, int nIndex) => GetWindowLong32(hWnd, nIndex);

    [DllImport("user32.dll")]
    public static extern IntPtr GetSystemMenu(IntPtr hwnd, bool bRevert);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnableMenuItem(IntPtr menu, uint uIDEnableItem, uint uEnable);

    [DllImport("User32", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowEnabled(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hwnd, out RectStruct lpRect);

    internal static void FindMaximumSingleMonitorRectangle(RectStruct windowRect, out RectStruct screenSubRect, out RectStruct monitorRect)
    {
        var rects = new List<RectStruct>();
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr _, ref RectStruct _, IntPtr _) =>
        {
            var monitorInfo = new MonitorInfoStruct
            {
                CbSize = (uint)Marshal.SizeOf(typeof(MonitorInfoStruct))
            };
            GetMonitorInfo(hMonitor, ref monitorInfo);
            rects.Add(monitorInfo.RcWork);
            return true;
        }, IntPtr.Zero);
        long currentArea = 0;
        screenSubRect = new RectStruct
        {
            Left = 0,
            Right = 0,
            Top = 0,
            Bottom = 0
        };
        monitorRect = new RectStruct
        {
            Left = 0,
            Right = 0,
            Top = 0,
            Bottom = 0
        };
        foreach (var rect in rects)
        {
            var lprcSrc1 = rect;
            IntersectRect(out var lprcDst, ref lprcSrc1, ref windowRect);
            var area = (long)(lprcDst.Width * lprcDst.Height);
            if (area > currentArea)
            {
                screenSubRect = lprcDst;
                monitorRect = rect;
                currentArea = area;
            }
        }
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();
}