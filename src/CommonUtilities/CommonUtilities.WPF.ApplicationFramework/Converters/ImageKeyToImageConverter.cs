using System.Globalization;
using AnakinRaW.CommonUtilities.Wpf.Converters;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Imaging.Controls;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Converters;

public class ImageKeyToImageConverter : MultiValueConverter<ImageKey, bool, ThemedImage>
{
    protected override ThemedImage? Convert(ImageKey imageKey, bool enabled, object parameter, CultureInfo culture)
    {
        if (imageKey == default)
            return null;
        var image = new ThemedImage
        {
            ImakgeKey = imageKey,
            Grayscale = !enabled
        };
        return image;
    }
}