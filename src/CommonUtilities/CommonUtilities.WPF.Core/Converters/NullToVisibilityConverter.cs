using System.Globalization;
using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.Converters;

public class NullToVisibilityConverter : ValueConverter<object?, Visibility>
{
    protected override Visibility Convert(object? value, object? parameter, CultureInfo culture)
    {
        return value is null ? Visibility.Collapsed : Visibility.Visible;
    }
}