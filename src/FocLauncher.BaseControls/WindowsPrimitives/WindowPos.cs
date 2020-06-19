using System;
using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    public class WindowPos
    {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public uint flags;
    }
}
