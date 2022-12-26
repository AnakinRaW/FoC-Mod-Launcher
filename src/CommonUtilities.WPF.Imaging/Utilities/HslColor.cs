using System;

using System.Windows.Media;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging;

internal struct HslColor
{
    private const double MinAlpha = 0.0;
    private const double MaxAlpha = 1.0;
    private const double MinHue = 0.0;
    private const double MaxHue = 360.0;
    private const double MinSaturation = 0.0;
    private const double MaxSaturation = 1.0;
    private const double MinLuminosity = 0.0;
    private const double MaxLuminosity = 1.0;
    private double _hue;
    private double _saturation;
    private double _luminosity;
    private double _alpha;

    public double Hue
    {
        get => _hue;
        set => _hue = LimitRange(value, MinHue, MaxHue);
    }

    public double Saturation
    {
        get => _saturation;
        set => _saturation = LimitRange(value, MinSaturation, MaxSaturation);
    }

    public double Luminosity
    {
        get => _luminosity;
        set => _luminosity = LimitRange(value, MinLuminosity, MaxLuminosity);
    }

    public double Alpha
    {
        get => _alpha;
        set => _alpha = LimitRange(value, MinAlpha, MaxAlpha);
    }

    public HslColor(double hue, double saturation, double luminosity)
        : this(hue, saturation, luminosity, 1.0)
    {
    }

    public HslColor(double hue, double saturation, double luminosity, double alpha)
    {
        _hue = LimitRange(hue, 0.0, MaxHue);
        _saturation = LimitRange(saturation, MinLuminosity, MaxSaturation);
        _luminosity = LimitRange(luminosity, MinSaturation, MaxLuminosity);
        _alpha = LimitRange(alpha, MinAlpha, MaxAlpha);
    }

    public static HslColor FromColor(Color color)
    {
        return FromArgb(color.A, color.R, color.G, color.B);
    }

    private static HslColor FromArgb(byte a, byte r, byte g, byte b)
    {
        var num1 = Math.Max(r, Math.Max(g, b));
        var num2 = Math.Min(r, Math.Min(g, b));
        double num3 = num1 - num2;
        var num4 = num1 / (double)byte.MaxValue;
        var num5 = num2 / (double)byte.MaxValue;
        double hue;
        if (num1 != num2)
            if (num1 != r)
                if (num1 != g)
                    hue = 60.0 * (r - g) / num3 + 240.0;
                else
                    hue = 60.0 * (b - r) / num3 + 120.0;
            else
                hue = (int)(60.0 * (g - b) / num3 + MaxHue) % 360;
        else
            hue = 0.0;
        var alpha = a / (double)byte.MaxValue;
        var luminosity = 0.5 * (num4 + num5);
        double saturation;
        if (num1 != num2)
            if (luminosity > 0.5)
                saturation = (num4 - num5) / (2.0 - 2.0 * luminosity);
            else
                saturation = (num4 - num5) / (2.0 * luminosity);
        else
            saturation = 0.0;
        return new HslColor(hue, saturation, luminosity, alpha);
    }

    public Color ToColor()
    {
        var (alpha, red, green, blue) = ToArgb();
        return Color.FromArgb(alpha, red, green, blue);
    }

    private (byte a, byte r, byte g, byte b) ToArgb()
    {
        double q;
        if (Luminosity < 0.5)
            q = Luminosity * (MaxSaturation + Saturation);
        else
            q = Luminosity + Saturation - Luminosity * Saturation;
        var p = 2.0 * Luminosity - q;
        var num = Hue / MaxHue;
        var tC1 = ModOne(num + 1.0 / 3.0);
        var tC3 = ModOne(num - 1.0 / 3.0);
        return ((byte)(Alpha * byte.MaxValue), (byte)(ComputeRgbComponent(p, q, tC1) * byte.MaxValue),
            (byte)(ComputeRgbComponent(p, q, num) * byte.MaxValue),
            (byte)(ComputeRgbComponent(p, q, tC3) * byte.MaxValue));
    }

    private static double ModOne(double value)
    {
        if (value < 0.0)
            return value + 1.0;
        return value > 1.0 ? value - 1.0 : value;
    }

    private static double ComputeRgbComponent(double p, double q, double tC)
    {
        if (tC < 1.0 / 6.0)
            return p + (q - p) * 6.0 * tC;
        if (tC < 0.5)
            return q;
        return tC < 2.0 / 3.0 ? p + (q - p) * 6.0 * (2.0 / 3.0 - tC) : p;
    }

    private static double LimitRange(double value, double min, double max)
    {
        value = Math.Max(min, value);
        value = Math.Min(value, max);
        return value;
    }
}