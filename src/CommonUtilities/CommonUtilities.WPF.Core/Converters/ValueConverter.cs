using System;
using System.Globalization;
using System.Windows.Data;
using Sklavenwalker.CommonUtilities.Wpf.Utilities;

namespace Sklavenwalker.CommonUtilities.Wpf.Converters;

public class ValueConverter<TSource, TTarget> : IValueConverter
{
    public object? Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == BindingOperations.DisconnectedSource)
            return value;
        if (value is not TSource && (value != null || typeof(TSource).IsValueType))
            throw this.MakeValueNotOfTypeException(typeof(TSource), value?.GetType()!, nameof(value), nameof(Convert));
        if (!targetType.IsAssignableFrom(typeof(TTarget)))
            throw this.MakeTargetNotExtendingTypeException(typeof(TTarget), targetType, nameof(Convert));
        return Convert((TSource)value!, parameter, culture);
    }

    public object? ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TTarget && (value != null || typeof(TTarget).IsValueType))
            throw this.MakeValueNotOfTypeException(typeof(TTarget), value?.GetType()!, nameof(value), nameof(ConvertBack));
        if (!targetType.IsAssignableFrom(typeof(TSource)))
            throw this.MakeTargetNotExtendingTypeException(typeof(TSource), targetType, nameof(ConvertBack));
        return ConvertBack((TTarget)value!, parameter, culture);
    }

    protected virtual TTarget? Convert(TSource value, object? parameter, CultureInfo culture)
    {
        throw this.MakeConverterFunctionNotDefinedException(nameof(Convert));
    }

    protected virtual TSource? ConvertBack(TTarget value, object? parameter, CultureInfo culture)
    {
        throw this.MakeConverterFunctionNotDefinedException(nameof(ConvertBack));
    }
}