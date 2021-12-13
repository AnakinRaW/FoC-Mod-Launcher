using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.NativeMethods;

internal static class HitTestConstants
{
    public const int HTNOWHERE = 0;
    public const int HTCLIENT = 1;
    public const int HTCAPTION = 2;
    public const int HTSYSMENU = 3;
    public const int HTMINBUTTON = 8;
    public const int HTMAXBUTTON = 9;
    public const int HTCLOSE = 20;
}

internal static class Shell32
{
    [DllImport("shell32.dll")]
    internal static extern IntPtr SHAppBarMessage(uint dwMessage, ref AppBarData pData);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    public static extern int ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);
}

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

    internal static IntPtr MakeParam(Point pt) => MakeParam((int)pt.X, (int)pt.Y);

    internal static int GetXLParam(int lParam) => LoWord(lParam);

    internal static int GetYLParam(int lParam) => HiWord(lParam);

    internal static int HiWord(int value) => (short)(value >> 16);

    internal static int HiWord(long value) => (short)(value >> 16 & ushort.MaxValue);

    internal static int HiWord(IntPtr value) => IntPtr.Size == 8 ? HiWord(value.ToInt64()) : HiWord(value.ToInt32());

    internal static int LoWord(int value) => (short)(value & ushort.MaxValue);

    internal static int LoWord(long value) => (short)(value & ushort.MaxValue);

    internal static int LoWord(IntPtr value) => IntPtr.Size == 8 ? LoWord(value.ToInt64()) : LoWord(value.ToInt32());

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

[return: MarshalAs(UnmanagedType.Bool)]
internal delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RectStruct lprcMonitor, IntPtr dwData);

public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

public enum GWL
{
    EXSTYLE = -20,
    STYLE = -16
}

[Flags]
public enum RedrawWindowFlags : uint
{
    Invalidate = 1,
    InternalPaint = 2,
    Erase = 4,
    Validate = 8,
    NoInternalPaint = 16, // 0x00000010
    NoErase = 32, // 0x00000020
    NoChildren = 64, // 0x00000040
    AllChildren = 128, // 0x00000080
    UpdateNow = 256, // 0x00000100
    EraseNow = 512, // 0x00000200
    Frame = 1024, // 0x00000400
    NoFrame = 2048, // 0x00000800
}

internal static class Kernel32
{
    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();
}

internal struct MinMaxInfoStruct
{
    public PointStruct PtReserved;
    public PointStruct PtMaxSize;
    public PointStruct PtMaxPosition;
    public PointStruct PtMinTrackSize;
    public PointStruct PtMaxTrackSize;
}

internal struct StyleStruct
{
    public int StyleOld;
    public int StyleNew;
}
