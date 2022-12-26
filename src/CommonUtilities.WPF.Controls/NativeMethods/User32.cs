using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.NativeMethods;

internal static class User32
{
    private static int _notifyOwnerActivate;

    public static int NotifyOwnerActive
    {
        get
        {
            if (_notifyOwnerActivate == 0)
                _notifyOwnerActivate = RegisterWindowMessage("NotifyOwnerActive{A982313C-756C-4da9-8BD0-0C375A45784B}");
            return _notifyOwnerActivate;
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int RegisterWindowMessage(string lpString);

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
        var point = new PointStruct { X = 0, Y = 0 };
        var cursorPos = new Point();
        if (GetCursorPos(ref point))
        {
            cursorPos.X = point.X;
            cursorPos.Y = point.Y;
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

    public static int GetWindowLong(IntPtr hWnd, GWL nIndex) => GetWindowLong(hWnd, (int)nIndex);


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

    internal static int HiWord(int value) => (short)(value >> 16);

    internal static int HiWord(long value) => (short)(value >> 16 & ushort.MaxValue);

    internal static int LoWord(int value) => (short)(value & ushort.MaxValue);

    internal static int LoWord(long value) => (short)(value & ushort.MaxValue);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetMessagePos();

    [DllImport("user32.dll")]
    internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpfn, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindow(IntPtr hwnd, int nCmd);

    internal static int GetXLParam(int lParam) => LoWord(lParam);

    internal static int GetYLParam(int lParam) => HiWord(lParam);

    internal static int ToInt32Unchecked(this IntPtr value) => (int)value.ToInt64();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ScreenToClient(IntPtr hWnd, ref PointStruct point);

    internal static IntPtr MakeParam(int lowWord, int highWord) => new(lowWord & ushort.MaxValue | highWord << 16);

    [DllImport("user32.dll")]
    internal static extern short GetKeyState(int vKey);

    internal static bool IsKeyPressed(int vKey) => GetKeyState(vKey) < 0;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hwnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsZoomed(IntPtr hwnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsIconic(IntPtr hwnd);

    [DllImport("user32.dll")]
    internal static extern int GetSystemMetrics(int index);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetClientRect(IntPtr hwnd, out RectStruct lpRect);

    [DllImport("user32.dll")]
    internal static extern int MapWindowPoints(IntPtr hwndFrom, IntPtr hwndTo, ref RectStruct rect, int points);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    internal static IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam) => SendMessage(hwnd, msg, wParam, IntPtr.Zero);

    [DllImport("user32.dll")]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);

    [DllImport("user32.dll")]
    internal static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PostMessage(IntPtr hWnd, int nMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowPlacement(IntPtr hwnd, WindowPlacementStruct lpwndpl);

    public static WindowPlacementStruct GetWindowPlacement(IntPtr hwnd)
    {
        var lpwndpl = new WindowPlacementStruct();
        return GetWindowPlacement(hwnd, lpwndpl) ? lpwndpl : throw new Win32Exception(Marshal.GetLastWin32Error());
    }
}