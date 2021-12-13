using System;
using System.Runtime.InteropServices;

namespace Sklavenwalker.Wpf.CommandBar.NativeMethods
{
    internal static class User32
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hwnd, int nCmd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern bool IsChild(IntPtr hWndParent, IntPtr hwnd);
    }
}
