using System;
using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    internal static class Msimg32
    {
        [DllImport("msimg32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AlphaBlend(IntPtr hdcDest, int xoriginDest, int yoriginDest, int wDest, int hDest,
            IntPtr hdcSrc, int xoriginSrc, int yoriginSrc, int wSrc, int hSrc, BlendFunction pfn);
    }
}
