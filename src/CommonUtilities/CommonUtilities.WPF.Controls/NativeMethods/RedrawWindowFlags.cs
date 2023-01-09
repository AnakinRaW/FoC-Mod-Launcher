using System;

namespace AnakinRaW.CommonUtilities.Wpf.NativeMethods;

[Flags]
public enum RedrawWindowFlags : uint
{
    Invalidate = 1,
    InternalPaint = 2,
    Erase = 4,
    Validate = 8,
    NoInternalPaint = 16, // 0x00000010
    NoErase = 32, // 0x00000020
    NoChildren = 64, // 0x00000040
    AllChildren = 128, // 0x00000080
    UpdateNow = 256, // 0x00000100
    EraseNow = 512, // 0x00000200
    Frame = 1024, // 0x00000400
    NoFrame = 2048, // 0x00000800
}