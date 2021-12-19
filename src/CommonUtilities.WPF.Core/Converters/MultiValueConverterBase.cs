using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace Sklavenwalker.CommonUtilities.Wpf.Converters;

public abstract class MultiValueConverterBase<TTarget> : IMultiValueConverter
{
    private readonly Type[] _genericArguments;

    protected MultiValueConverterBase(Type[] genericArguments)
    {
        _genericArguments = genericArguments;
    }

    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return !ValidateConvertParameters(values, targetType)
            ? default
            : ConvertCore(values, targetType, parameter, culture);
    }

    protected abstract TTarget? ConvertCore(object[] values, Type targetType, object parameter, CultureInfo culture);

    public object?[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        ValidateConvertBackParameters(value, targetTypes);
        return ConvertBackCore(value, targetTypes, parameter, culture);
    }

    protected abstract object?[] ConvertBackCore(object value, Type[] targetTypes, object parameter, CultureInfo culture);

    private int ExpectedSourceValueCount => _genericArguments.Length - 1;

    protected bool ValidateConvertParameters(object[] values, Type targetType)
    {
        var caller = "Convert";
        if (values.Length != ExpectedSourceValueCount)
            throw this.MakeInsufficientSourceParametersException(ExpectedSourceValueCount, values.Length, caller);
        foreach (var obj in values)
        {
            if (obj == DependencyProperty.UnsetValue || obj == BindingOperations.DisconnectedSource)
                return false;
        }
        if (!targetType.IsAssignableFrom(typeof(TTarget)))
            throw this.MakeTargetNotExtendingTypeException(typeof(TTarget), targetType, caller);
        return true;
    }

    protected void ValidateConvertBackParameters(object? value, Type[] targetTypes)
    {
        const string caller = "ConvertBack";
        if (targetTypes.Length != ExpectedSourceValueCount)
            throw this.MakeInsufficientTypeParametersException(ExpectedSourceValueCount, targetTypes.Length, caller);
        if (value is not TTarget && (value != null || typeof(TTarget).IsValueType))
            throw this.MakeValueNotOfTypeException(typeof(TTarget), value?.GetType(), nameof(value), caller);
        for (var offset = 0; offset < targetTypes.Length; ++offset)
        {
            var targetType = targetTypes[offset];
            var genericArgument = _genericArguments[offset];
            if (!targetType.IsAssignableFrom(genericArgument))
                throw this.MakeTargetAtOffsetNotExtendingTypeException(genericArgument, targetType, offset, caller);
        }
    }

    protected T? CheckValue<T>(object?[] values, int index, [CallerMemberName] string caller = "")
    {
        return values[index] is T || values[index] == null
            ? (T)values[index]!
            : throw this.MakeValueAtOffsetNotOfTypeException(typeof(T), values[index]?.GetType(), nameof(values), index,
                caller);
    }
}