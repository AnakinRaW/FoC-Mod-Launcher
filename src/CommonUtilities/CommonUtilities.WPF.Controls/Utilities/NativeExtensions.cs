using System;

namespace AnakinRaW.CommonUtilities.Wpf.Utilities;

internal static class NativeExtensions
{
    internal static int GetXLParam(int lParam)
    {
        return LoWord(lParam);
    }

    internal static int GetYLParam(int lParam)
    {
        return HiWord(lParam);
    }

    internal static int HiWord(int value)
    {
        return (short)(value >> 16);
    }

    internal static int HiWord(long value)
    {
        return (short)(value >> 16 & ushort.MaxValue);
    }

    internal static int LoWord(int value)
    {
        return (short)(value & ushort.MaxValue);
    }

    internal static int LoWord(long value)
    {
        return (short)(value & ushort.MaxValue);
    }

    internal static int ToInt32Unchecked(this IntPtr value)
    {
        return (int)value.ToInt64();
    }

    internal static IntPtr MakeParam(int lowWord, int highWord)
    {
        return new(lowWord & ushort.MaxValue | highWord << 16);
    }
}