namespace Sklavenwalker.CommonUtilities.Wpf.Imaging.Converters;

//public class DpiPrescaleThemedImageSourceConverter : MultiValueConverter<ImageSource, Color, bool, ImageSource>
//{ 
//    protected override ImageSource? Convert(ImageSource? inputImage, Color backgroundColor, bool isEnabled, object parameter, CultureInfo culture)
//    {
//        return InternalConvert(inputImage, backgroundColor, isEnabled, parameter, culture);
//    }

//    internal static ImageSource? InternalConvert(ImageSource? inputImage, Color backgroundColor, bool isEnabled, object parameter, CultureInfo culture)
//    {
//        return InternalConvert(inputImage, backgroundColor, isEnabled, SystemParameters.HighContrast, parameter, culture);
//    }

//    internal static ImageSource? InternalConvert(ImageSource? inputImage, Color backgroundColor, bool isEnabled, bool isHighContrast, object parameter, CultureInfo culture)
//    {
//        return DpiPrescaleImageSourceConverter.InternalConvert(
//            ThemedImageSourceConverter.ConvertCore(inputImage, backgroundColor, isEnabled, isHighContrast, parameter));
//    }
//}


//public class DpiPrescaleImageSourceConverter : ValueConverter<ImageSource, ImageSource>
//{
//    protected override ImageSource? Convert(ImageSource inputImage, object? parameter, CultureInfo culture)
//    {
//        return InternalConvert(inputImage);
//    }

//    internal static ImageSource? InternalConvert(ImageSource? inputImage)
//    {
//        if (inputImage == null)
//            return null;
//        var num = DpiAwareness.SystemDpiX / 96.0;
//        if (num <= 1.0)
//            return inputImage;
//        var imageHandle = ImageLibrary.Instance.AddCustomImage(inputImage, false);
//        var attributes = new ImageAttributes(new Size(inputImage.Width * num, inputImage.Height * num));
//        return ImageLibrary.Instance.GetImage(imageHandle.Moniker, attributes);
//    }
//}

//public class ThemedImageSourceConverter : MultiValueConverter<ImageSource, Color, bool, ImageSource>
//{
//    internal static ImageSource? ConvertCore(ImageSource? inputImage, Color backgroundColor, bool isEnabled, bool isHighContrast,
//        bool? enhanceContrastIfNecessary, object parameter)
//    {
//        if (inputImage is not BitmapSource inputImage1 || backgroundColor.A == 0 && isEnabled)
//            return inputImage;
//        var biasColor = GrayscaleBitmapSourceConverter.GetBiasColor(parameter);
//        return !enhanceContrastIfNecessary.HasValue ? (ImageSource)ImageThemingUtilities.GetOrCreateThemedBitmapSource(inputImage1, backgroundColor, isEnabled, biasColor, isHighContrast) : (ImageSource)ImageThemingUtilities.GetOrCreateThemedBitmapSource(inputImage1, backgroundColor, isEnabled, biasColor, isHighContrast, enhanceContrastIfNecessary.Value);
//    }

//    public static ImageSource? ConvertCore(ImageSource? inputImage, Color backgroundColor, bool isEnabled, bool isHighContrast, object parameter)
//    {
//        return ConvertCore(inputImage, backgroundColor, isEnabled, isHighContrast, new bool?(), parameter);
//    }

//    public static ImageSource? ConvertCore(ImageSource? inputImage, Color backgroundColor, bool isEnabled, object parameter)
//    {
//        return ConvertCore(inputImage, backgroundColor, isEnabled, SystemParameters.HighContrast, parameter);
//    }

//    protected override ImageSource? Convert(ImageSource? inputImage, Color backgroundColor, bool isEnabled, object parameter, CultureInfo culture)
//    {
//        return ConvertCore(inputImage, backgroundColor, isEnabled, parameter);
//    }
//}

//public class GrayscaleBitmapSourceConverter : ValueConverter<BitmapSource, BitmapSource>
//{
//    private static readonly Color DefaultBiasColor = Colors.White;
//    private const int BytesPerPixelBgra32 = 4;

//    protected override BitmapSource? Convert(BitmapSource image, object? parameter, CultureInfo culture)
//    {
//        return ConvertCore(image, GetBiasColor(parameter));
//    }

//    public static BitmapSource? ConvertCore(BitmapSource? image, Color biasColor)
//    {
//        if (image == null)
//            return null;
//        BitmapSource bitmapSource;
//        if (image.Format == PixelFormats.Bgra32 && image.PixelWidth <= 128 && image.PixelHeight <= 128)
//        {
//            bitmapSource = ConvertToGrayScale(image, biasColor);
//        }
//        else
//        {
//            if (biasColor.R != byte.MaxValue || biasColor.G != byte.MaxValue || biasColor.B != byte.MaxValue)
//                throw new NotSupportedException("Specifying non-white bias color is not supported for images with PixelFormat other than BGRA32, or larger than 128x128.");
//            var formatConvertedBitmap = new FormatConvertedBitmap();
//            formatConvertedBitmap.BeginInit();
//            formatConvertedBitmap.DestinationFormat = PixelFormats.Gray32Float;
//            formatConvertedBitmap.Source = image;
//            formatConvertedBitmap.EndInit();
//            if (formatConvertedBitmap.CanFreeze)
//                formatConvertedBitmap.Freeze();
//            bitmapSource = formatConvertedBitmap;
//        }
//        return bitmapSource;
//    }

//    public static Color GetBiasColor(object? parameter)
//    {
//        return parameter is Color color ? color : DefaultBiasColor;
//    }

//    private static unsafe BitmapSource ConvertToGrayScale(
//      BitmapSource image,
//      Color biasColor)
//    {
//        Requires.NotNull((object)image, nameof(image));
//        if (image.Format != PixelFormats.Bgra32)
//            throw new ArgumentException("Image is not the expected type", nameof(image));
//        var stride = image.PixelWidth * BytesPerPixelBgra32;
//        var num = image.PixelWidth * image.PixelHeight * BytesPerPixelBgra32;

//        var arrayPool = ArrayPool<byte>.Shared;
//        var resource = arrayPool.Rent(num);
//        try
//        {
//            image.CopyPixels(resource, stride, 0);
//            ImageThemingUtilities.GrayscaleDIBits(resource, num, biasColor);
//            BitmapSource grayScale;
//            fixed (byte* buffer = resource)
//                grayScale = BitmapSource.Create(image.PixelWidth, image.PixelHeight, image.DpiX, image.DpiY,
//                    PixelFormats.Bgra32, image.Palette, (IntPtr)buffer, num, stride);
//            grayScale.Freeze();
//            return grayScale;
//        }
//        finally
//        {
//            arrayPool.Return(resource);
//        }
//    }
//}