using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Imaging.Utilities;
using Validation;
using Color = System.Windows.Media.Color;

namespace AnakinRaW.CommonUtilities.Wpf.Imaging;

public static class ImageThemingUtilities
{
    public static readonly DependencyProperty ImageBackgroundColorProperty = DependencyProperty.RegisterAttached(
        "ImageBackgroundColor", typeof(Color), typeof(ImageThemingUtilities),
        new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.Inherits));

    private const float BlueChannelWeight = 0.0004296875f;
    private const float GreenChannelWeight = 0.0023046874f;
    private const float RedChannelWeight = 0.001171875f;
    private const double HighContrastCutOffLuminosity = 0.3;

    private static readonly ConcurrentDictionary<WeakImageCacheKey, ConditionalWeakTable<BitmapSource, BitmapSource>>
        WeakImageCache = new();

    public static bool IsImageThemingEnabled { get; set; }

    static ImageThemingUtilities()
    {
        IsImageThemingEnabled = true;
    }

    public static void ClearWeakImageCache()
    {
        WeakImageCache.Clear();
    }

    public static Color GetImageBackgroundColor(DependencyObject obj)
    {
        return (Color)obj.GetValue(ImageBackgroundColorProperty);
    }

    public static void SetImageBackgroundColor(DependencyObject obj, Color value)
    {
        obj.SetValue(ImageBackgroundColorProperty, value);
    }

    public static bool IsDark(this Color color)
    {
        return HslColor.FromColor(color).Luminosity < 0.5;
    }

    public static bool IsLight(this Color color)
    {
        return !color.IsDark();
    }

    public static BitmapSource GetOrCreateThemedBitmapSource(BitmapSource inputImage, Color backgroundColor,
        bool isEnabled, Color grayscaleBiasColor, bool isHighContrast)
    {
        return GetOrCreateThemedBitmapSource(inputImage, backgroundColor, isEnabled, grayscaleBiasColor, isHighContrast, false);
    }

    public static BitmapSource GetOrCreateThemedBitmapSource(BitmapSource inputImage, Color backgroundColor,
        bool isEnabled, Color grayscaleBiasColor, bool isHighContrast, bool enhanceContrastIfNecessary)
    {
        Requires.NotNull(inputImage, nameof(inputImage));
        var key = new WeakImageCacheKey(backgroundColor, grayscaleBiasColor, isEnabled);
        return WeakImageCache.GetOrAdd(key, _ => new ConditionalWeakTable<BitmapSource, BitmapSource>())
            .GetValue(inputImage, innerInputImage => CreateThemedBitmapSource(innerInputImage, backgroundColor,
                isEnabled, grayscaleBiasColor, isHighContrast, enhanceContrastIfNecessary));
    }

    public static void GrayscalePixelsWithBias(Span<byte> pixels, int pixelLength, Color biasColor)
    {
        if (pixelLength % 4 != 0)
            throw new ArgumentException("Expected pixels in BGRA32 format.", nameof(pixels));
        var alphaMask = biasColor.A / 256f;
        for (var index = 0; index + 4 <= pixelLength; index += 4)
        {
            var newPixel = pixels[index] * BlueChannelWeight + 
                           pixels[index + 1] * GreenChannelWeight +
                           pixels[index + 2] * RedChannelWeight;

            pixels[index] = (byte)(newPixel * (double)biasColor.B);
            pixels[index + 1] = (byte)(newPixel * (double)biasColor.G);
            pixels[index + 2] = (byte)(newPixel * (double)biasColor.R);
            pixels[index + 3] = (byte)(alphaMask * (double)pixels[index + 3]);
        }
    }

    private static bool ShouldRenderIconsWithEnhancedContrast(HslColor background)
    {
        var num1 = 0.4 * (1.0 - background.Saturation) + 0.7 * background.Saturation;
        var num2 = 0.5 - num1 / 2.0;
        var num3 = 0.5 + num1 / 2.0;
        return background.Luminosity >= num2 && background.Luminosity <= num3;
    }

    public static bool ThemePixels(int pixelCount, Span<byte> pixels, int width, int height,
        uint backgroundRgba, bool isHighContrast, bool enhanceContrastIfNecessary)
    {
        if (pixelCount != width * height * 4)
            throw new ArgumentException("Expected pixels in BGRA32 format.", nameof(pixels));
        if (!IsImageThemingEnabled)
            return false;

        var backgroundColor = backgroundRgba.ToColorFromRgba();
        var background = HslColor.FromColor(backgroundColor);
        if (enhanceContrastIfNecessary && !HasAlphaChannel(pixelCount, pixels))
            enhanceContrastIfNecessary = false;

        if (enhanceContrastIfNecessary && ShouldRenderIconsWithEnhancedContrast(background))
        {
            for (var index = 0; index + 4 <= pixelCount; index += 4)
                ThemePixelWithExtraContrast(ref pixels[index + 2], ref pixels[index + 1], ref pixels[index], background);
        }
        else
        {
            for (var index = 0; index + 4 <= pixelCount; index += 4)
            {
                if (isHighContrast && enhanceContrastIfNecessary)
                    ThemePixelWithExtraContrast(ref pixels[index + 2], ref pixels[index + 1], ref pixels[index], background);
                else 
                    ThemePixelWithoutExtraContrast(ref pixels[index + 2], ref pixels[index + 1], ref pixels[index], background, isHighContrast);
            }
        }
        return true;
    }

    private static void ThemePixelWithoutExtraContrast(ref byte r, ref byte g, ref byte b, HslColor background, bool isHighContrast)
    {
        var hslColor = HslColor.FromColor(Color.FromRgb(r, g, b));
        var hue1 = hslColor.Hue;
        var saturation = hslColor.Saturation;
        var luminosity = hslColor.Luminosity;
        var num1 = Math.Abs(82.0 / 85.0 - luminosity);
        var num2 = Math.Max(0.0, 1.0 - saturation * 4.0) * Math.Max(0.0, 1.0 - num1 * 4.0);
        var num3 = Math.Max(0.0, 1.0 - saturation * 4.0);
        var transformLuminosity = TransformLuminosity(hue1, saturation, luminosity, background);
        var hue2 = hue1 * (1.0 - num3) + background.Hue * num3;
        var saturation2 = saturation * (1.0 - num2) + background.Saturation * num2;
        if (isHighContrast)
            transformLuminosity =
                (transformLuminosity <= HighContrastCutOffLuminosity ? 0.0 : transformLuminosity >= 0.7 ? 1.0 : (transformLuminosity - HighContrastCutOffLuminosity) / 0.4) *
                (1.0 - saturation2) + transformLuminosity * saturation2;
        var color = new HslColor(hue2, saturation2, transformLuminosity).ToColor();
        r = color.R;
        g = color.G;
        b = color.B;
    }

    private static double TransformLuminosity(double hue, double saturation, double luminosity, HslColor background)
    {
        var backgroundLuminosity = background.Luminosity;
        if (background.ToColor().IsDark())
        {
            if (luminosity >= 82.0 / 85.0)
                return backgroundLuminosity * (luminosity - 1.0) / (-3.0 / 85.0);
            var val2 = saturation >= 0.2 ? saturation <= HighContrastCutOffLuminosity ? 1.0 - (saturation - 0.2) / (1.0 / 10.0) : 0.0 : 1.0;
            var num1 = Math.Max(Math.Min(1.0, Math.Abs(hue - 37.0) / 20.0), val2);
            var num2 = ((backgroundLuminosity - 1.0) * 0.66 / (82.0 / 85.0) + 1.0) * num1 + 0.66 * (1.0 - num1);
            return luminosity < 0.66 ? (num2 - 1.0) / 0.66 * luminosity + 1.0 : (num2 - backgroundLuminosity) / (-259.0 / 850.0) * (luminosity - 82.0 / 85.0) + backgroundLuminosity;
        }
        return luminosity < 82.0 / 85.0 ? luminosity * backgroundLuminosity / (82.0 / 85.0) : (1.0 - backgroundLuminosity) * (luminosity - 1.0) / (3.0 / 85.0) + 1.0;
    }

    private static void ThemePixelWithExtraContrast(ref byte r, ref byte g, ref byte b, HslColor background)
    {
        var num = HslColor.FromColor(Color.FromRgb(r, g, b)).Luminosity > 0.6
            ? byte.MaxValue
            : (byte)0;
        if (background.ToColor().IsDark())
            num = (byte)(byte.MaxValue - (uint)num);
        r = num;
        g = num;
        b = num;
    }

    private static bool HasAlphaChannel(int pixelCount, ReadOnlySpan<byte> pixels)
    {
        for (var index = 0; index + 4 <= pixelCount; index += 4)
        {
            switch (pixels[index + 3])
            {
                case 0:
                case byte.MaxValue:
                    continue;
                default:
                    return true;
            }
        }
        return false;
    }

    private static unsafe BitmapSource CreateThemedBitmapSource(BitmapSource inputImage, Color backgroundColor,
        bool isEnabled, Color grayscaleBiasColor, bool isHighContrast, bool enhanceContrastIfNecessary)
    {
        if (inputImage.Format != PixelFormats.Bgra32)
            inputImage = new FormatConvertedBitmap(inputImage, PixelFormats.Bgra32, null, 0.0);
        var stride = inputImage.PixelWidth * 4;
        var pixelCount = inputImage.PixelWidth * inputImage.PixelHeight * 4;

        var pool = ArrayPool<byte>.Shared;
        var pixels = pool.Rent(pixelCount);
        try
        {
            inputImage.CopyPixels(pixels, stride, 0);
            var backgroundRgba = (uint)(backgroundColor.B << 16 | backgroundColor.G << 8) | backgroundColor.R;
            var enhanceContrast = enhanceContrastIfNecessary && isEnabled;
            ThemePixels(pixelCount, pixels, inputImage.PixelWidth, inputImage.PixelHeight,
                backgroundRgba, isHighContrast, enhanceContrast);
            if (!isEnabled)
                GrayscalePixelsWithBias(pixels, pixelCount, grayscaleBiasColor);
            BitmapSource themedBitmapSource;
            fixed (byte* buffer = pixels)
                themedBitmapSource = BitmapSource.Create(inputImage.PixelWidth, inputImage.PixelHeight, inputImage.DpiX,
                    inputImage.DpiY, PixelFormats.Bgra32, inputImage.Palette, (IntPtr)buffer, pixelCount, stride);
            themedBitmapSource.Freeze();
            return themedBitmapSource;
        }
        finally
        {
            pool.Return(pixels);
        }
    }

    private static int ComputeOffsetToOptOutPixel(int width, int height, bool isTopDownBitmap)
    {
        return isTopDownBitmap ? width - 1 : width * height - 1;
    }

    private struct WeakImageCacheKey : IEquatable<WeakImageCacheKey>
    {
        private readonly Color _background;
        private readonly Color _grayscaleBias;
        private readonly bool _isEnabled;

        public WeakImageCacheKey(Color background, Color grayscaleBias, bool isEnabled)
        {
            _background = background;
            _grayscaleBias = isEnabled ? Colors.Transparent : grayscaleBias;
            _isEnabled = isEnabled;
        }

        public override string ToString()
        {
            return $"{_background}, {_grayscaleBias}, {(_isEnabled ? "enabled" : "disabled")}";
        }

        public override bool Equals(object? obj)
        {
            return obj is WeakImageCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_background, _grayscaleBias, _isEnabled);
        }

        public bool Equals(WeakImageCacheKey other)
        {
            return _background == other._background && _grayscaleBias == other._grayscaleBias && _isEnabled == other._isEnabled;
        }
    }
}