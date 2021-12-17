using System.Globalization;
using Sklavenwalker.CommonUtilities.Wpf.Converters;
using Sklavenwalker.CommonUtilities.Wpf.Imaging.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging.Converters;

public sealed class ActualDpiConverter : ValueConverter<double, double>
{
    public static readonly ActualDpiConverter Instance = new();

    protected override double Convert(double dpi, object? parameter, CultureInfo culture) => AreClose(dpi, 0.0) ? ThemedImage.DefaultDpi : dpi;

    private static bool AreClose(double value1, double value2)
    {
        if (IsNonreal(value1) || IsNonreal(value2))
            return value1.CompareTo(value2) == 0;
        if (value1 == value2)
            return true;
        var num = value1 - value2;
        return num < 1.53E-06 && num > -1.53E-06;
    }

    private static bool IsNonreal(double value)
    {
        return double.IsNaN(value) || double.IsInfinity(value);
    }
}