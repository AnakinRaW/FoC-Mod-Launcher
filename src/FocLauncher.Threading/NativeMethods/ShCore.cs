using System;
using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    internal static class ShCore
    {
        [DllImport("shcore.dll")]
        internal static extern uint GetProcessDpiAwareness(IntPtr process, out ProcessDpiAwareness awareness);

        [DllImport("shcore.dll")]
        internal static extern int GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);
    }
}