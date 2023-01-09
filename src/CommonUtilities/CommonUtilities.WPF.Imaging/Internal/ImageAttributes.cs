using System;
using System.Windows;
using System.Windows.Media;

namespace AnakinRaW.CommonUtilities.Wpf.Imaging;

internal struct ImageAttributes : IEquatable<ImageAttributes>
{
    public Color GrayscaleBiasColor { get; }
    public Size DeviceSize { get; }
    public Color Background { get; }
    public bool Grayscale { get; }

    public bool HighContrast { get; set; }

    public ImageAttributes(Size deviceSize) : this(deviceSize, default, false, ImageLibrary.DefaultGrayscaleBiasColor, false)
    {
    }

    public ImageAttributes(Size deviceSize, Color background, bool grayscale, Color? grayscaleBiasColor, bool highContrast)
    {
        DeviceSize = deviceSize;
        Background = background;
        HighContrast = highContrast;

        if (grayscaleBiasColor.HasValue)
            GrayscaleBiasColor = grayscaleBiasColor.Value;
        else
            GrayscaleBiasColor = HighContrast ? ImageLibrary.HighContrastGrayscaleBiasColor : ImageLibrary.DefaultGrayscaleBiasColor;
        Grayscale = grayscale;
    }

    public override bool Equals(object? other)
    {
        return other is ImageAttributes imageAttributes && Equals(imageAttributes);
    }

    public bool Equals(ImageAttributes other)
    {
        return GrayscaleBiasColor == other.GrayscaleBiasColor && DeviceSize.Equals(other.DeviceSize) &&
               Background == other.Background && HighContrast == other.HighContrast && Grayscale == other.Grayscale;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GrayscaleBiasColor, DeviceSize, Background, HighContrast, Grayscale);
    }

    public static bool operator ==(ImageAttributes attr1, ImageAttributes attr2)
    {
        return attr1.Equals(attr2);
    }

    public static bool operator !=(ImageAttributes attr1, ImageAttributes attr2)
    {
        return !attr1.Equals(attr2);
    }
}