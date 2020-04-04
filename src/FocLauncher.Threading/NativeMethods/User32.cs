using System;
using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    internal static class User32
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        internal static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
    }
}