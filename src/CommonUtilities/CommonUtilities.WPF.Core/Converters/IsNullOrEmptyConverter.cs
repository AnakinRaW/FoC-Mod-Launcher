using System.Globalization;

namespace Sklavenwalker.CommonUtilities.Wpf.Converters;

public sealed class IsNullOrEmptyConverter : ToBooleanValueConverter<string>
{
    protected override bool Convert(string value, object parameter, CultureInfo culture) => string.IsNullOrEmpty(value);
}