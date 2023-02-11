using System;
using System.Globalization;

namespace AnakinRaW.CommonUtilities.Wpf.Converters;

internal class ProgressToPercentConverter : ValueConverter<double, string>
{
    protected override string? Convert(double value, object? parameter, CultureInfo culture)
    {
        return $"{Math.Floor(value)}%";
    }
}