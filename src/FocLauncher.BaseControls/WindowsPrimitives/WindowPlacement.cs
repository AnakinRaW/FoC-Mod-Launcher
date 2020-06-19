using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    internal class WindowPlacement
    {
        public int flags;
        public int length = Marshal.SizeOf(typeof(WindowPlacement));
        public POINT ptMaxPosition;
        public POINT ptMinPosition;
        public RECT rcNormalPosition;
        public int showCmd;
    }
}