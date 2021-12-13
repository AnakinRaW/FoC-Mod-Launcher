using System;
using System.Runtime.InteropServices;
using Sklavenwalker.CommonUtilities.Wpf.DPI;

namespace Sklavenwalker.CommonUtilities.Wpf.NativeMethods;

internal static class ShCore
{
    [DllImport("shcore.dll")]
    internal static extern int GetProcessDpiAwareness(IntPtr process, out DpiAwareness.ProcessDpiAwareness awareness);

    [DllImport("shcore.dll")]
    internal static extern int GetDpiForMonitor(IntPtr hmonitor, DpiAwareness.MonitorDpiType dpiType, out uint dpiX, out uint dpiY);
}