using System;
using System.Runtime.InteropServices;

namespace AnakinRaW.CommonUtilities.Wpf.NativeMethods;

internal static class DwmApi
{
    [DllImport("dwmapi.dll")]
    internal static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, ref int attrValue, int attrSize);
}