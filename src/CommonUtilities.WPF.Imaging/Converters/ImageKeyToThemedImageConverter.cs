using System.Globalization;
using Sklavenwalker.CommonUtilities.Wpf.Converters;
using Sklavenwalker.CommonUtilities.Wpf.Imaging.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging.Converters;

public class ImageKeyToThemedImageConverter : ValueConverter<ImageKey, ThemedImage>
{
    protected override ThemedImage Convert(ImageKey source, object? parameter, CultureInfo culture)
    {
        return new ThemedImage { ImakgeKey = source };
    }
}