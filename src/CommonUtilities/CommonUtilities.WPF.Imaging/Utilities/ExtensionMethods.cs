using System.Windows.Media;

namespace AnakinRaW.CommonUtilities.Wpf.Imaging.Utilities;

internal static class ExtensionMethods
{
    public static Color ToColorFromRgba(this uint colorValue)
    {
        return Color.FromArgb((byte)(colorValue >> 24), (byte)colorValue, (byte)(colorValue >> 8), (byte)(colorValue >> 16));
    }
}