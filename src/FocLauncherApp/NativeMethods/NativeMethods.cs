using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FocLauncherApp.NativeMethods
{
    internal static class NativeMethods
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("wininet.dll", SetLastError = true)]
        internal static extern bool InternetGetConnectedState(out ConnectionStates lpdwFlags, int dwReserved);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindow(IntPtr hwnd, int nCmd);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr hwnd);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int nMsg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetGUIThreadInfo(uint idThread, out GuiThreadInfo lpgui);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        
        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("User32", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo monitorInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetProp(IntPtr hwnd, string propName, IntPtr value);

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorOpts dwFlags);

        [DllImport("shcore.dll")]
        internal static extern int GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

        [DllImport("shcore.dll")]
        internal static extern uint GetProcessDpiAwareness(IntPtr process, out ProcessDpiAwareness awareness);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetThreadDpiAwarenessContext();

        [DllImport("user32.dll")]
        internal static extern bool AreDpiAwarenessContextsEqual(IntPtr dpiContextA, IntPtr dpiContextB);
        
        [DllImport("gdi32.dll")]
        internal static extern int GetDeviceCaps(IntPtr hdc, DeviceCaps index);

        public static string GetWindowText(IntPtr hwnd)
        {
            var lpString = new StringBuilder(GetWindowTextLength(hwnd) + 1);
            GetWindowText(hwnd, lpString, lpString.Capacity);
            return lpString.ToString();
        }

        [Flags]
        internal enum ConnectionStates
        {
            Modem = 0x1,
            LAN = 0x2,
            Proxy = 0x4,
            RasInstalled = 0x10,
            Offline = 0x20,
            Configured = 0x40,
        }
    }
}
