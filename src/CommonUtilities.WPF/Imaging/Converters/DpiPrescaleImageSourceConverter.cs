using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.Converters;
using Sklavenwalker.CommonUtilities.Wpf.DPI;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging.Converters;

public class DpiPrescaleImageSourceConverter : ValueConverter<ImageSource, ImageSource>
{
    protected override ImageSource? Convert(ImageSource inputImage, object? parameter, CultureInfo culture)
    {
        return InternalConvert(inputImage);
    }

    internal static ImageSource? InternalConvert(ImageSource? inputImage)
    {
        if (inputImage == null)
            return null;
        var num = DpiAwareness.SystemDpiX / 96.0;
        if (num <= 1.0)
            return inputImage;
        var moniker = ImageLibrary.Instance.AddCustomImage(inputImage, false);
        var attributes = new ImageAttributes(new Size(inputImage.Width * num, inputImage.Height * num));
        return ImageLibrary.Instance.GetImage(moniker, attributes);
    }
}