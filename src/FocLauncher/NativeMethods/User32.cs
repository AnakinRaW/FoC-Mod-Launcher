using System;
using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    internal static class User32
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetMessagePos();
        
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyIcon(IntPtr hIcon);
    }
}