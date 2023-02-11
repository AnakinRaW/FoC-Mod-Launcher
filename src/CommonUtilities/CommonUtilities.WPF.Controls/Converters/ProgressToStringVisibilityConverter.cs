using System.Globalization;
using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.Converters;

internal class ProgressToStringVisibilityConverter : ValueConverter<double, Visibility>
{
    public bool Invert { get; set; }

    protected override Visibility Convert(double value, object? parameter, CultureInfo culture)
    {
        return value >= 100.0 ^ Invert ? Visibility.Collapsed : Visibility.Visible;
    }
}