using System.Globalization;
using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.Converters;

public sealed class StringToVisibilityConverter : ValueConverter<string, Visibility>
{
    protected override Visibility Convert(string value, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrWhiteSpace(value) ? Visibility.Visible : Visibility.Collapsed;
    }
}