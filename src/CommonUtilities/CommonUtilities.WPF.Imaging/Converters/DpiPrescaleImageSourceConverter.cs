using System.Globalization;
using System.Windows;
using System.Windows.Media;
using AnakinRaW.CommonUtilities.Wpf.Converters;
using AnakinRaW.CommonUtilities.Wpf.DPI;

namespace AnakinRaW.CommonUtilities.Wpf.Imaging.Converters;

// TODO: Can they be deleted?
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
        var num = DpiHelper.SystemDpiX / 96.0;
        if (num <= 1.0)
            return inputImage;
        var imageKey = ImageLibrary.Instance.AddCustomImage(inputImage, false);
        var attributes = new ImageAttributes(new Size(inputImage.Width * num, inputImage.Height * num));
        return ImageLibrary.Instance.GetImage(imageKey, attributes);
    }
}