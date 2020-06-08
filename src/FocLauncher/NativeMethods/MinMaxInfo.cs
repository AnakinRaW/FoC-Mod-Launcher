using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MinMaxInfo
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }
}