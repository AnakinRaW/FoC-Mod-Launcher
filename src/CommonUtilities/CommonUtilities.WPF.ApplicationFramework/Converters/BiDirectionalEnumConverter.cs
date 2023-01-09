using System;
using System.Globalization;
using System.Windows.Data;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Converters;

internal class BiDirectionalEnumConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;
        if (targetType.IsEnum)
            return Enum.ToObject(targetType, value);
        return value.GetType().IsEnum ? System.Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())) : null;
    }

    public object? ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}