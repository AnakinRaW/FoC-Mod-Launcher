using System.Globalization;
using Sklavenwalker.CommonUtilities.Wpf.Converters;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.Imaging.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Converters;

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