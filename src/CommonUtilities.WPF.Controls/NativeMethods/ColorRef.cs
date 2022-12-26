using System.Windows.Media;

namespace Sklavenwalker.CommonUtilities.Wpf.NativeMethods;

internal struct ColorRef
{
    public uint DwColor;

    public ColorRef(uint dwColor) => DwColor = dwColor;

    public ColorRef(Color color) => DwColor = (uint)(color.R + (color.G << 8) + (color.B << 16));

    public Color GetMediaColor() => Color.FromRgb((byte)(byte.MaxValue & DwColor), (byte)((65280U & DwColor) >> 8), (byte)((16711680U & DwColor) >> 16));
}