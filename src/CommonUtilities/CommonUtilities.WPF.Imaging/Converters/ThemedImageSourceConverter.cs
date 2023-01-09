using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Converters;

namespace AnakinRaW.CommonUtilities.Wpf.Imaging.Converters;

public class ThemedImageSourceConverter : MultiValueConverter<ImageSource, Color, bool, ImageSource>
{
    internal static ImageSource? ConvertCore(ImageSource? inputImage, Color backgroundColor, bool isEnabled, bool isHighContrast,
        bool? enhanceContrastIfNecessary, object parameter)
    {
        if (inputImage is not BitmapSource bitmapSource || backgroundColor.A == 0 && isEnabled)
            return inputImage;
        var biasColor = GrayscaleBitmapSourceConverter.GetBiasColor(parameter);
        return !enhanceContrastIfNecessary.HasValue
            ? ImageThemingUtilities.GetOrCreateThemedBitmapSource(bitmapSource, backgroundColor, isEnabled, biasColor, isHighContrast)
            : ImageThemingUtilities.GetOrCreateThemedBitmapSource(bitmapSource, backgroundColor, isEnabled, biasColor, isHighContrast, enhanceContrastIfNecessary.Value);
    }

    public static ImageSource? ConvertCore(ImageSource? inputImage, Color backgroundColor, bool isEnabled, bool isHighContrast, object parameter)
    {
        return ConvertCore(inputImage, backgroundColor, isEnabled, isHighContrast, new bool?(), parameter);
    }

    public static ImageSource? ConvertCore(ImageSource? inputImage, Color backgroundColor, bool isEnabled, object parameter)
    {
        return ConvertCore(inputImage, backgroundColor, isEnabled, SystemParameters.HighContrast, parameter);
    }

    protected override ImageSource? Convert(ImageSource? inputImage, Color backgroundColor, bool isEnabled, object parameter, CultureInfo culture)
    {
        return ConvertCore(inputImage, backgroundColor, isEnabled, parameter);
    }
}