using System;
using System.Runtime.InteropServices;

namespace AnakinRaW.CommonUtilities.Wpf.NativeMethods;

internal static class Shell32
{
    [DllImport("shell32.dll")]
    internal static extern IntPtr SHAppBarMessage(uint dwMessage, ref AppBarData pData);
}