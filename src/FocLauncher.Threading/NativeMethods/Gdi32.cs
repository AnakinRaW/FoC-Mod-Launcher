using System;
using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    internal static class Gdi32
    {
        internal const int LogPixelsX = 88;
        internal const int LogPixelsY = 90;

        [DllImport("gdi32.dll")]
        internal static extern int GetDeviceCaps(IntPtr hdc, int index);
    }
}