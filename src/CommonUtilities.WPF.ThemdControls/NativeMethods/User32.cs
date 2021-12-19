using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

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

    [DllImport("User32", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);
    
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoStruct monitorInfo);
    
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
    internal static extern IntPtr GetSystemMenu(IntPtr hwnd, bool bRevert);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowPlacement(IntPtr hwnd, WindowPlacementStruct lpwndpl);

    public static WindowPlacementStruct GetWindowPlacement(IntPtr hwnd)
    { 
        var lpwndpl = new WindowPlacementStruct();
        return GetWindowPlacement(hwnd, lpwndpl) ? lpwndpl : throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool EnableMenuItem(IntPtr menu, uint uIDEnableItem, uint uEnable);

    [DllImport("user32.dll")]
    internal static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PostMessage(IntPtr hWnd, int nMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    public static int GetWindowLong(IntPtr hWnd, GWL nIndex) => GetWindowLong(hWnd, (int)nIndex);

    [DllImport("user32.dll")]
    public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int nMsg, IntPtr wParam, IntPtr lParam);

    internal static IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam) => SendMessage(hwnd, msg, wParam, IntPtr.Zero);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpfn, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindow(IntPtr hwnd, int nCmd);

    [DllImport("user32.dll")]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int SetWindowLong(IntPtr hWnd, short nIndex, int value);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    internal static int ToInt32Unchecked(this IntPtr value) => (int)value.ToInt64();

    internal static IntPtr MakeParam(int lowWord, int highWord) => new(lowWord & ushort.MaxValue | highWord << 16);

    internal static int GetXLParam(int lParam) => LoWord(lParam);

    internal static int GetYLParam(int lParam) => HiWord(lParam);

    internal static int HiWord(int value) => (short)(value >> 16);

    internal static int HiWord(long value) => (short)(value >> 16 & ushort.MaxValue);
    
    internal static int LoWord(int value) => (short)(value & ushort.MaxValue);

    internal static int LoWord(long value) => (short)(value & ushort.MaxValue);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ScreenToClient(IntPtr hWnd, ref PointStruct point);

    [DllImport("user32.dll")]
    internal static extern short GetKeyState(int vKey);

    internal static bool IsKeyPressed(int vKey) => GetKeyState(vKey) < 0;

    [DllImport("user32.dll")]
    internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetClientRect(IntPtr hwnd, out RectStruct lpRect);

    [DllImport("user32.dll")]
    internal static extern int MapWindowPoints(IntPtr hwndFrom, IntPtr hwndTo, ref RectStruct rect, int points);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetMessagePos();
}