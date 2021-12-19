using System;
using System.Buffers;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.Converters;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging.Converters;

public class GrayscaleBitmapSourceConverter : ValueConverter<BitmapSource, BitmapSource>
{
    private static readonly Color DefaultBiasColor = Colors.White;
    private const int BytesPerPixelBgra32 = 4;

    protected override BitmapSource? Convert(BitmapSource image, object? parameter, CultureInfo culture)
    {
        return ConvertCore(image, GetBiasColor(parameter));
    }

    public static BitmapSource? ConvertCore(BitmapSource? image, Color biasColor)
    {
        if (image == null)
            return null;
        BitmapSource bitmapSource;
        if (image.Format == PixelFormats.Bgra32 && image.PixelWidth <= 128 && image.PixelHeight <= 128)
        {
            bitmapSource = ConvertToGrayScale(image, biasColor);
        }
        else
        {
            if (biasColor.R != byte.MaxValue || biasColor.G != byte.MaxValue || biasColor.B != byte.MaxValue)
                throw new NotSupportedException("Specifying non-white bias color is not supported for images with PixelFormat other than BGRA32, or larger than 128x128.");
            var formatConvertedBitmap = new FormatConvertedBitmap();
            formatConvertedBitmap.BeginInit();
            formatConvertedBitmap.DestinationFormat = PixelFormats.Gray32Float;
            formatConvertedBitmap.Source = image;
            formatConvertedBitmap.EndInit();
            if (formatConvertedBitmap.CanFreeze)
                formatConvertedBitmap.Freeze();
            bitmapSource = formatConvertedBitmap;
        }
        return bitmapSource;
    }

    public static Color GetBiasColor(object? parameter)
    {
        return parameter is Color color ? color : DefaultBiasColor;
    }

    private static unsafe BitmapSource ConvertToGrayScale(
        BitmapSource image,
        Color biasColor)
    {
        Requires.NotNull((object)image, nameof(image));
        if (image.Format != PixelFormats.Bgra32)
            throw new ArgumentException("Image is not the expected type", nameof(image));
        var stride = image.PixelWidth * BytesPerPixelBgra32;
        var num = image.PixelWidth * image.PixelHeight * BytesPerPixelBgra32;

        var arrayPool = ArrayPool<byte>.Shared;
        var resource = arrayPool.Rent(num);
        try
        {
            image.CopyPixels(resource, stride, 0);
            ImageThemingUtilities.GrayscaleDiBits(resource, num, biasColor);
            BitmapSource grayScale;
            fixed (byte* buffer = resource)
                grayScale = BitmapSource.Create(image.PixelWidth, image.PixelHeight, image.DpiX, image.DpiY,
                    PixelFormats.Bgra32, image.Palette, (IntPtr)buffer, num, stride);
            grayScale.Freeze();
            return grayScale;
        }
        finally
        {
            arrayPool.Return(resource);
        }
    }
}