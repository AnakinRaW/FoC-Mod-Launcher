using System.Globalization;

namespace Sklavenwalker.CommonUtilities.Wpf.Converters;

public class InverseBooleanConverter : ValueConverter<bool, bool>
{
    protected override bool Convert(bool value, object? parameter, CultureInfo culture)
    {
        return !value;
    }
}