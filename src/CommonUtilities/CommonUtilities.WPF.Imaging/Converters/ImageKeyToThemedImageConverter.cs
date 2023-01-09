using System.Globalization;
using AnakinRaW.CommonUtilities.Wpf.Converters;
using AnakinRaW.CommonUtilities.Wpf.Imaging.Controls;

namespace AnakinRaW.CommonUtilities.Wpf.Imaging.Converters;

public class ImageKeyToThemedImageConverter : ValueConverter<ImageKey, ThemedImage>
{
    protected override ThemedImage Convert(ImageKey source, object? parameter, CultureInfo culture)
    {
        return new ThemedImage { ImakgeKey = source };
    }
}