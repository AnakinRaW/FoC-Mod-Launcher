using System;

namespace FocLauncherHost.NativeMethods
{
    internal struct GuiThreadInfo
    {
        public int CbSize;
        public int Flags;
        public IntPtr HwndActive;
        public IntPtr HwndFocus;
        public IntPtr HwndCapture;
        public IntPtr HwndMenuOwner;
        public IntPtr HwndMoveSize;
        public IntPtr HwndCaret;
        public RECT RcCaret;
    }
}