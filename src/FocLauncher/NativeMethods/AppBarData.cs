using System;
using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AppBarData
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uCallbackMessage;
        public int uEdge;
        public RECT rc;
        public bool lParam;
    }
}