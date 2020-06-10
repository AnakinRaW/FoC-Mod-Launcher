using System;
using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    internal static class Shell32
    {
        [DllImport("shell32", CallingConvention = CallingConvention.StdCall)]
        internal static extern int SHAppBarMessage(uint dwMessage, ref AppBarData pData);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);
    }
}