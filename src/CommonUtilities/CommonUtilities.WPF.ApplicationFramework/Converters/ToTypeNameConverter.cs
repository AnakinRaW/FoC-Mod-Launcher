using System.Globalization;
using AnakinRaW.CommonUtilities.Wpf.Converters;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Converters;

internal sealed class ToTypeNameConverter : ValueConverter<object?, string>
{
    protected override string? Convert(object? value, object? parameter, CultureInfo culture)
    {
        return value is null ? "(null)" : value.GetType().FullName;
    }
}