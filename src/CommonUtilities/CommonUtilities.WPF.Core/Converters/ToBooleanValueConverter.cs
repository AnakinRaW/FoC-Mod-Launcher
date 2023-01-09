using System;
using System.Globalization;
using System.Windows.Data;

namespace AnakinRaW.CommonUtilities.Wpf.Converters;

public class ToBooleanValueConverter<TSource> : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TSource && (value != null || typeof(TSource).IsValueType))
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Value must be of type {0}.", typeof(TSource).FullName));
        if (!targetType.IsAssignableFrom(typeof(bool)))
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Target type must extend {0}.", typeof(bool).FullName));
        return Convert((TSource)value!, parameter, culture);
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool b)
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Value must be of type {0}.", typeof(bool).FullName));
        if (!targetType.IsAssignableFrom(typeof(TSource)))
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Target type must extend {0}.", typeof(TSource).FullName));
        return ConvertBack(b, parameter, culture);
    }

    protected virtual bool Convert(TSource value, object parameter, CultureInfo culture) => throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "{0} is not defined for this converter.", nameof(Convert)));

    protected virtual TSource? ConvertBack(bool value, object parameter, CultureInfo culture) => throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "{0} is not defined for this converter.", nameof(ConvertBack)));
}