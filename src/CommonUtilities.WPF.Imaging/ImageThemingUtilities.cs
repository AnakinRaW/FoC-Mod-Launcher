using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Validation;
using Color = System.Windows.Media.Color;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging;

public static class ImageThemingUtilities
{
    public unsafe delegate bool ModifyPixelCallback(BitmapSource originalSource, byte* pixels);

    public static readonly DependencyProperty ImageBackgroundColorProperty = DependencyProperty.RegisterAttached(
        "ImageBackgroundColor", typeof(Color), typeof(ImageThemingUtilities),
        new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.Inherits));

    private const double BlueChannelWeight = 0.000429687497671694;
    private const double GreenChannelWeight = 0.00230468739755452;
    private const double RedChannelWeight = 0.00117187504656613;
    private const double HighContrastCutOffLuminosity = 0.3;

    private static readonly ConcurrentDictionary<WeakImageCacheKey, ConditionalWeakTable<BitmapSource, BitmapSource>>
        WeakImageCache = new();

    public static bool IsImageThemingEnabled { get; set; }

    static ImageThemingUtilities()
    {
        IsImageThemingEnabled = true;
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

    public static unsafe BitmapSource SetOptOutPixel(BitmapSource source)
    {
        return ModifyBitmap(source, (s, pixels) =>
        {
            SetOptOutPixel(pixels, s.PixelWidth, s.PixelHeight, true);
            return true;
        });
    }

    public static unsafe void SetOptOutPixel(byte* pPixelBytes, int width, int height, bool isTopDownBitmap)
    {
        var offsetToOptOutPixel = ComputeOffsetToOptOutPixel(width, height, isTopDownBitmap);
        *(int*)(pPixelBytes + ((IntPtr)(offsetToOptOutPixel * 4)).ToInt64()) = unchecked((int)0xFF00FFFF);
    }

    public static unsafe BitmapSource ModifyBitmap(BitmapSource source, ModifyPixelCallback modifier)
    {
        Requires.NotNull(source, nameof(source));
        if (source.Format.BitsPerPixel != 32)
            source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0.0);
        var stride = source.PixelWidth * (source.Format.BitsPerPixel / 8);
        var pixels1 = new uint[source.PixelWidth * source.PixelHeight];
        source.CopyPixels(pixels1, stride, 0);
        fixed (uint* pixels2 = pixels1)
        {
            if (!modifier(source, (byte*)pixels2))
                return source;
        }

        source = BitmapSource.Create(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, source.Format,
            source.Palette, pixels1, stride);
        return source;
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

    public static void GrayscaleDiBits(byte[] pixels, int pixelLength, Color biasColor)
    {
        Requires.NotNull((object)pixels, nameof(pixels));
        if (pixelLength % 4 != 0)
            throw new ArgumentException("The supplied bitmap bits do not represent a complete BGRA32 bitmap.", nameof(pixels));
        var num1 = biasColor.A / 256f;
        for (var index = 0; index + 4 <= pixelLength; index += 4)
        {
            var newPixel = (float)(pixels[index] * BlueChannelWeight + pixels[index + 1] * GreenChannelWeight +
                               pixels[index + 2] * RedChannelWeight);
            pixels[index] = (byte)(newPixel * (double)biasColor.B);
            pixels[index + 1] = (byte)(newPixel * (double)biasColor.G);
            pixels[index + 2] = (byte)(newPixel * (double)biasColor.R);
            pixels[index + 3] = (byte)(num1 * (double)pixels[index + 3]);
        }
    }

    public static bool ThemeDiBits(int pixelCount, byte[]? pixels, int width, int height, bool isTopDownBitmap,
        uint backgroundRgba, bool isHighContrast, bool enhanceContrastIfNecessary)
    {
        if (pixelCount != width * height * 4)
            throw new ArgumentException();
        if (!IsImageThemingEnabled || pixels == null)
            return false;
        if (IsOptOutPixelSet(pixels, width, height, isTopDownBitmap))
        {
            ClearOptOutPixel(pixels, width, height, isTopDownBitmap);
            return false;
        }

        var backgroundColor = backgroundRgba.ToColorFromRgba();
        var background = HslColor.FromColor(backgroundColor);
        if (enhanceContrastIfNecessary && !HasAlphaChannel(pixelCount, pixels))
            enhanceContrastIfNecessary = false;


        var pool = ArrayPool<byte>.Shared;
        byte[] buffer;

        if (enhanceContrastIfNecessary)
        {
            buffer = pool.Rent(pixelCount);
            Array.Copy(pixels, buffer, pixelCount);
        }
        else
            buffer = pool.Rent(0);


        for (var index = 0; index + 4 <= pixelCount; index += 4)
        {
            ThemePixelWithoutExtraContrast(ref pixels[index + 2],
                ref pixels[index + 1], ref pixels[index], background, isHighContrast);
        }

        var contrastEvaluator = new ContrastEvaluator(pixels, width, height, backgroundColor, 126, 0.5, 3.0);


        if (enhanceContrastIfNecessary)
        {
            if (!contrastEvaluator.ImageHasSufficientContrast())
            {
                Array.Copy(buffer, pixels, pixelCount);
                for (var index = 0; index + 4 <= pixelCount; index += 4)
                    ThemePixelWithExtraContrast(ref pixels[index + 2],
                        ref pixels[index + 1], ref pixels[index], background);
            }
        }

        return true;
    }

    private static void ThemePixelWithoutExtraContrast(ref byte r, ref byte g, ref byte b, HslColor background, bool isHighContrast)
    {
        var hslColor = HslColor.FromColor(Color.FromRgb(r, g, b));
        var hue1 = hslColor.Hue;
        var saturation1 = hslColor.Saturation;
        var luminosity1 = hslColor.Luminosity;
        var num1 = Math.Abs(82.0 / 85.0 - luminosity1);
        var num2 = Math.Max(0.0, 1.0 - saturation1 * 4.0) * Math.Max(0.0, 1.0 - num1 * 4.0);
        var num3 = Math.Max(0.0, 1.0 - saturation1 * 4.0);
        var luminosity2 = TransformLuminosity(hue1, saturation1, luminosity1, background);
        var hue2 = hue1 * (1.0 - num3) + background.Hue * num3;
        var saturation2 = saturation1 * (1.0 - num2) + background.Saturation * num2;
        if (isHighContrast)
            luminosity2 =
                (luminosity2 <= HighContrastCutOffLuminosity ? 0.0 : luminosity2 >= 0.7 ? 1.0 : (luminosity2 - HighContrastCutOffLuminosity) / 0.4) *
                (1.0 - saturation2) + luminosity2 * saturation2;
        var color = new HslColor(hue2, saturation2, luminosity2).ToColor();
        r = color.R;
        g = color.G;
        b = color.B;
    }

    private static double TransformLuminosity(double hue, double saturation, double luminosity, HslColor background)
    {
        var luminosity1 = background.Luminosity;
        if (background.ToColor().IsDark())
        {
            if (luminosity >= 82.0 / 85.0)
                return luminosity1 * (luminosity - 1.0) / (-3.0 / 85.0);
            var val2 = saturation >= 0.2 ? saturation <= HighContrastCutOffLuminosity ? 1.0 - (saturation - 0.2) / (1.0 / 10.0) : 0.0 : 1.0;
            var num1 = Math.Max(Math.Min(1.0, Math.Abs(hue - 37.0) / 20.0), val2);
            var num2 = ((luminosity1 - 1.0) * 0.66 / (82.0 / 85.0) + 1.0) * num1 + 0.66 * (1.0 - num1);
            return luminosity < 0.66 ? (num2 - 1.0) / 0.66 * luminosity + 1.0 : (num2 - luminosity1) / (-259.0 / 850.0) * (luminosity - 82.0 / 85.0) + luminosity1;
        }
        return luminosity < 82.0 / 85.0 ? luminosity * luminosity1 / (82.0 / 85.0) : (1.0 - luminosity1) * (luminosity - 1.0) / (3.0 / 85.0) + 1.0;
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

    public static unsafe bool IsOptOutPixelSet(byte[] pixels, int width, int height, bool isTopDownBitmap)
    {
        fixed (byte* pPixelBytes = pixels)
            return IsOptOutPixelSet(pPixelBytes, width, height, isTopDownBitmap);
    }

    public static unsafe bool IsOptOutPixelSet(byte* pPixelBytes, int width, int height, bool isTopDownBitmap)
    {
        var offsetToOptOutPixel = ComputeOffsetToOptOutPixel(width, height, isTopDownBitmap);
        return (int)(*(uint*)(pPixelBytes + ((IntPtr)(offsetToOptOutPixel * 4)).ToInt64()) & 16777215U) == ushort.MaxValue;
    }

    public static unsafe void ClearOptOutPixel(byte[] pixels, int width, int height, bool isTopDownBitmap)
    {
        fixed (byte* pPixelBytes = pixels)
            ClearOptOutPixel(pPixelBytes, width, height, isTopDownBitmap);
    }

    public static unsafe void ClearOptOutPixel(byte* pPixelBytes, int width, int height, bool isTopDownBitmap)
    {
        var offsetToOptOutPixel = ComputeOffsetToOptOutPixel(width, height, isTopDownBitmap);
        *(int*)(pPixelBytes + ((IntPtr)(offsetToOptOutPixel * 4)).ToInt64()) = 0;
    }

    private static unsafe BitmapSource CreateThemedBitmapSource(BitmapSource inputImage, Color backgroundColor,
        bool isEnabled, Color grayscaleBiasColor, bool isHighContrast, bool enhanceContrastIfNecessary)
    {
        if (inputImage.Format != PixelFormats.Bgra32)
            inputImage = new FormatConvertedBitmap(inputImage, PixelFormats.Bgra32, null, 0.0);
        var stride = inputImage.PixelWidth * 4;
        var num = inputImage.PixelWidth * inputImage.PixelHeight * 4;

        var pool = ArrayPool<byte>.Shared;
        var resource = pool.Rent(num);
        try
        {
            inputImage.CopyPixels(resource, stride, 0);
            var backgroundRgba = (uint)(backgroundColor.B << 16 | backgroundColor.G << 8) | backgroundColor.R;
            var enhanceContrastIfNecessary1 = enhanceContrastIfNecessary & isEnabled;
            ThemeDiBits(num, resource, inputImage.PixelWidth, inputImage.PixelHeight, true,
                backgroundRgba, isHighContrast, enhanceContrastIfNecessary1);
            if (!isEnabled)
                GrayscaleDiBits(resource, num, grayscaleBiasColor);
            BitmapSource themedBitmapSource;
            fixed (byte* buffer = resource)
                themedBitmapSource = BitmapSource.Create(inputImage.PixelWidth, inputImage.PixelHeight, inputImage.DpiX,
                    inputImage.DpiY, PixelFormats.Bgra32, inputImage.Palette, (IntPtr)buffer, num, stride);
            themedBitmapSource.Freeze();
            return themedBitmapSource;
        }
        finally
        {
            pool.Return(resource);
        }
    }

    private static int ComputeOffsetToOptOutPixel(int width, int height, bool isTopDownBitmap)
    {
        return isTopDownBitmap ? width - 1 : width * height - 1;
    }

    private struct WeakImageCacheKey : IEquatable<WeakImageCacheKey>
    {
        private Color Background;
        private Color GrayscaleBias;
        private readonly bool IsEnabled;

        public WeakImageCacheKey(Color background, Color grayscaleBias, bool isEnabled)
        {
            Background = background;
            GrayscaleBias = isEnabled ? Colors.Transparent : grayscaleBias;
            IsEnabled = isEnabled;
        }

        public override string ToString()
        {
            return $"{Background}, {GrayscaleBias}, {(IsEnabled ? "enabled" : "disabled")}";
        }

        public override bool Equals(object obj)
        {
            return obj is WeakImageCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Background, GrayscaleBias, IsEnabled);
        }

        public bool Equals(WeakImageCacheKey other)
        {
            return Background == other.Background && GrayscaleBias == other.GrayscaleBias && IsEnabled == other.IsEnabled;
        }
    }
}