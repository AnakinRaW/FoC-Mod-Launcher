using System;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.Wpf.DPI;

namespace AnakinRaW.CommonUtilities.Wpf.NativeMethods;

internal static class ShCore
{
    [DllImport("shcore.dll")]
    internal static extern int GetProcessDpiAwareness(IntPtr process, out DpiHelper.ProcessDpiAwareness awareness);

    [DllImport("shcore.dll")]
    internal static extern int GetDpiForMonitor(IntPtr hmonitor, DpiHelper.MonitorDpiType dpiType, out uint dpiX, out uint dpiY);
}