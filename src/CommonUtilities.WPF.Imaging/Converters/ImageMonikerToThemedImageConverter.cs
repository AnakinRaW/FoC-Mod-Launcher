using System.Globalization;
using Sklavenwalker.CommonUtilities.Wpf.Converters;
using Sklavenwalker.CommonUtilities.Wpf.Imaging.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging.Converters;

public class ImageMonikerToThemedImageConverter : ValueConverter<ImageMoniker, ThemedImage>
{
    protected override ThemedImage Convert(ImageMoniker source, object? parameter, CultureInfo culture)
    {
        return new ThemedImage { Moniker = source };
    }
}