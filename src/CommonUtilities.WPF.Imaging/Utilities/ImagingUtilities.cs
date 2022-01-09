using System;
using System.IO;
using System.Windows;
using System.Windows.Baml2006;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.Imaging.Converters;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging.Utilities;

internal static class ImagingUtilities
{
    internal static void ValidateAttributes(ImageAttributes attributes)
    {
        if (attributes.DeviceSize.IsEmpty || attributes.DeviceSize.Width == 0 || attributes.DeviceSize.Height == 0)
            throw new ArgumentException($"Invalid image size: {attributes.DeviceSize}");
    }

    public static BitmapSource? LoadImage(ImageAttributes attributes, ImageDefinition imageDefinition)
    {
        var image = LoadManagedImage(imageDefinition, attributes.DeviceSize);
        var themedImage = ThemeImage(image, attributes, imageDefinition.CanTheme);
        themedImage = DetectAndRemoveOptOutPixel(themedImage);
        if (themedImage is { CanFreeze: true })
            themedImage.Freeze();
        return themedImage;
    }
    
    private static BitmapSource? ThemeImage(BitmapSource? image, ImageAttributes attributes, bool canTheme)
    {
        if (image == null)
            return null;
        if (canTheme)
            image = ThemedImageSourceConverter.ConvertCore(image, attributes.Background, !attributes.Grayscale,
                attributes.HighContrast, false, attributes.GrayscaleBiasColor) as BitmapSource;
        else if (attributes.Grayscale)
            image = GrayscaleBitmapSourceConverter.ConvertCore(image, attributes.GrayscaleBiasColor);
        if (!canTheme)
            image = ImageThemingUtilities.SetOptOutPixel(image!);
        return image;
    }

    private static unsafe BitmapSource? DetectAndRemoveOptOutPixel(BitmapSource? source)
    {
        if (source is null)
            return null;
        var bitmapSource = ImageThemingUtilities.ModifyBitmap(source, (s, pixels) =>
        {
            var wasThemed = !ImageThemingUtilities.IsOptOutPixelSet(pixels, s.PixelWidth, s.PixelHeight, true);
            if (wasThemed)
                return false;
            ImageThemingUtilities.ClearOptOutPixel(pixels, s.PixelWidth, s.PixelHeight, true);
            return true;
        });
        return bitmapSource;
    }

    private static BitmapSource? LoadManagedImage(ImageDefinition imageDefinition, Size deviceSize)
    {
        var uri = imageDefinition.Source;
        Stream? stream = null;
        try
        {
            return FetchResourceStream(uri, out stream) switch
            {
                UriResourceType.Unknown => null,
                UriResourceType.Png => LoadBitmappedImage(stream!, deviceSize),
                UriResourceType.Baml => LoadXamlImage(stream!, deviceSize),
                UriResourceType.Xaml => LoadXamlImage(uri, deviceSize),
                _ => null
            };
        }
        finally
        {
            stream?.Close();
        }
    }

    private static BitmapSource? LoadXamlImage(Stream stream, Size deviceSize)
    {
        Requires.NotNull(stream, nameof(stream));
        return Application.Current.Dispatcher.Invoke(() =>
        {
            using var reader = new Baml2006Reader(stream);
            var imageAsObject = XamlReader.Load(reader);
            var bitmapSource = imageAsObject != null ? LoadXamlImage(imageAsObject, deviceSize) : null;
            return bitmapSource;
        });
    }

    private static BitmapSource? LoadXamlImage(Uri source, Size deviceSize)
    {
        return Application.Current.Dispatcher.Invoke(() =>
        {
            var imageAsObject = Application.LoadComponent(source);
            return imageAsObject != null ? LoadXamlImage(imageAsObject, deviceSize) : null;
        });
    }

    private static BitmapSource? LoadXamlImage(object imageAsObject, Size deviceSize)
    {
        Requires.NotNull(imageAsObject, nameof(imageAsObject));
        return imageAsObject switch
        {
            BitmapSource image => StretchImage(image, deviceSize),
            FrameworkElement element => RasterizeElement(element, deviceSize),
            _ => throw new Exception("Unsupported element type. The element must be a UIElement or a BitmapSource.")
        };
    }
    
    private static BitmapSource? LoadBitmappedImage(Stream stream, Size deviceSize)
    {
        Requires.NotNull(stream, nameof(stream));
        var bitmapDecoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
        if (bitmapDecoder is null)
            return null;
        var image = (BitmapSource)bitmapDecoder.Frames[0];
        return Application.Current.Dispatcher.Invoke(() => StretchImage(image, deviceSize));
    }

    private static BitmapScalingMode SelectScalingMode(int scalingPercent)
    { 
        return scalingPercent % 100 == 0 ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.HighQuality;
    }

    private static BitmapSource? StretchImage(BitmapSource? imageSource, Size deviceSize)
    {
        if (imageSource == null)
            return null;
        if ((int)deviceSize.Width == imageSource.PixelWidth && (int)deviceSize.Height == imageSource.PixelHeight)
        {
            if (imageSource.CanFreeze)
                imageSource.Freeze();
            return imageSource;
        }
        var image = new Image
        {
            Source = imageSource
        };
        var bitmapScalingMode = SelectScalingMode((int)Math.Round(100.0 * deviceSize.Width / imageSource.PixelWidth));
        RenderOptions.SetBitmapScalingMode(image, bitmapScalingMode);
        return RasterizeElement(image, deviceSize);
    }

    private static BitmapSource RasterizeElement(FrameworkElement element, Size deviceSize)
    {
        var rect = new Rect(0.0, 0.0, deviceSize.Width, deviceSize.Height);
        var source = new RenderTargetBitmap((int)deviceSize.Width, (int)deviceSize.Height, 96.0, 96.0, PixelFormats.Pbgra32);
        element.Width = rect.Width;
        element.Height = rect.Height;
        element.Measure(deviceSize);
        element.Arrange(rect);
        var drawingVisual = new DrawingVisual();
        using (var drawingContext = drawingVisual.RenderOpen())
        {
            var visualBrush = new VisualBrush(element) { Stretch = Stretch.Uniform };
            drawingContext.DrawRectangle(visualBrush, null, rect);
        }
        source.Render(drawingVisual);
        var bitmapSource = (BitmapSource)new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0.0);
        if (bitmapSource.CanFreeze)
            bitmapSource.Freeze();
        return bitmapSource;
    }

    private static UriResourceType FetchResourceStream(Uri uri, out Stream? stream)
    {
        stream = null;
        var resourceStream = Application.GetResourceStream(uri);
        if (resourceStream == null)
            return UriResourceType.Unknown;
        var resourceType = GetResourceType(resourceStream.ContentType);
        if (resourceType == UriResourceType.Unknown)
            throw new Exception($"Unsupported content type: {resourceStream.ContentType}");
        stream = resourceStream.Stream;
        return resourceType;
    }

    private static UriResourceType GetResourceType(string contentType)
    {
        var lowerInvariant = contentType.ToLowerInvariant();
        return lowerInvariant switch
        {
            "application/xaml+xml" => UriResourceType.Xaml,
            "application/baml+xml" => UriResourceType.Baml,
            "image/png" => UriResourceType.Png,
            _ => UriResourceType.Unknown
        };
    }

    private enum UriResourceType
    {
        Unknown,
        Png,
        Baml,
        Xaml,
    }
}