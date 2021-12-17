using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.Converters;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging.Converters;

internal sealed class CrispImageSourceConverter : 
    MultiValueConverter<ImageMoniker, double, double, Color, bool, Color, bool, double, double, ImageSource>
{
    protected override ImageSource? Convert(ImageMoniker moniker, double logicalWidth, double logicalHeight, 
        Color background, bool grayscale, Color biasColor, bool highContrast, double dpi, double scaleFactor, object parameter, CultureInfo culture)
    {
        try
        {
            return ConvertCore(moniker, logicalWidth, logicalHeight, background, grayscale, biasColor, highContrast, dpi, scaleFactor);
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static ImageSource? ConvertCore(ImageMoniker moniker, double logicalWidth, double logicalHeight, 
        Color background, bool grayscale, Color biasColor, bool highContrast, double dpi, double scaleFactor)
    {
        if (moniker == ImageLibrary.InvalidImageMoniker)
            return null;
        var num = dpi / 96.0 * scaleFactor;
        ValidateDimensions(logicalWidth, logicalHeight, 0.0, short.MaxValue / num);
        var attributes = new ImageAttributes(
            new Size(logicalWidth * num, logicalHeight * num),
            background,
            grayscale,
            biasColor,
            highContrast);
        return ImageLibrary.Instance.GetImage(moniker, attributes);
    }

    private static void ValidateDimensions(double logicalWidth, double logicalHeight, double logicalMin, double logicalMax)
    {
        if (IsNonreal(logicalWidth))
            throw new ArgumentException("Value must be a real number.", nameof(logicalWidth));
        if (IsNonreal(logicalHeight))
            throw new ArgumentException("Value must be a real number.", nameof(logicalHeight));
        if (LessThanOrClose(logicalWidth, logicalMin))
            throw new ArgumentException($"Value {logicalMin} is too small.", nameof(logicalWidth));
        if (LessThanOrClose(logicalHeight, logicalMin))
            throw new ArgumentException($"Value {logicalMin} is too small.", nameof(logicalHeight));
        if (GreaterThanOrClose(logicalWidth, logicalMax))
            throw new ArgumentException($"Value {logicalMax} is too large.", nameof(logicalWidth));
        if (GreaterThanOrClose(logicalHeight, logicalMax))
            throw new ArgumentException($"Value {logicalMax} is too large.", nameof(logicalHeight));
    }

    private static bool AreClose(double value1, double value2)
    {
        if (IsNonreal(value1) || IsNonreal(value2))
            return value1.CompareTo(value2) == 0;
        if (value1 == value2)
            return true;
        var num = value1 - value2;
        return num is < 1.53E-06 and > -1.53E-06;
    }

    private static bool LessThanOrClose(double value1, double value2)
    {
        return value1 < value2 || AreClose(value1, value2);
    }

    private static bool GreaterThanOrClose(double value1, double value2)
    {
        return value1 > value2 || AreClose(value1, value2);
    }

    private static bool IsNonreal(double value)
    {
        return double.IsNaN(value) || double.IsInfinity(value);
    }
}