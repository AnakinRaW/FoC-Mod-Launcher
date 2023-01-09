using System.Globalization;
using System.Windows;
using System.Windows.Media;
using AnakinRaW.CommonUtilities.Wpf.Converters;

namespace AnakinRaW.CommonUtilities.Wpf.Imaging.Converters;

// TODO: Can they be deleted?
public class DpiPrescaleThemedImageSourceConverter : MultiValueConverter<ImageSource, Color, bool, ImageSource>
{
    protected override ImageSource? Convert(ImageSource? inputImage, Color backgroundColor, bool isEnabled, object parameter, CultureInfo culture)
    {
        return InternalConvert(inputImage, backgroundColor, isEnabled, parameter, culture);
    }

    internal static ImageSource? InternalConvert(ImageSource? inputImage, Color backgroundColor, bool isEnabled, object parameter, CultureInfo culture)
    {
        return InternalConvert(inputImage, backgroundColor, isEnabled, SystemParameters.HighContrast, parameter, culture);
    }

    internal static ImageSource? InternalConvert(ImageSource? inputImage, Color backgroundColor, bool isEnabled, bool isHighContrast, object parameter, CultureInfo culture)
    {
        return DpiPrescaleImageSourceConverter.InternalConvert(
            ThemedImageSourceConverter.ConvertCore(inputImage, backgroundColor, isEnabled, isHighContrast, parameter));
    }
}