using System;

namespace FocLauncher.NativeMethods
{
    [Flags]
    internal enum RedrawWindowFlags : uint
    {
        Invalidate = 1U,
        InternalPaint = 2U,
        Erase = 4U,
        Validate = 8U,
        NoInternalPaint = 16U,
        NoErase = 32U,
        NoChildren = 64U,
        AllChildren = 128U,
        UpdateNow = 256U,
        EraseNow = 512U,
        Frame = 1024U,
        NoFrame = 2048U
    }
}