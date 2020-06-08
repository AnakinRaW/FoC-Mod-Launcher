using System.Runtime.InteropServices;

namespace FocLauncher.NativeMethods
{
    internal struct BitmapInfo
    {
        internal int BiSize;
        internal int BiWidth;
        internal int BiHeight;
        internal short BiPlanes;
        internal short BiBitCount;
        internal int BiCompression;
        internal int BiSizeImage;
        internal int BiXPelsPerMeter;
        internal int BiYPelsPerMeter;
        internal int BiClrUsed;
        internal int BiClrImportant;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        internal byte[] BmiColors;

        internal static BitmapInfo Default => new BitmapInfo
        {
            BiSize = 40,
            BiPlanes = 1
        };
    }
}
