using System;
using System.Globalization;

namespace Sklavenwalker.CommonUtilities.Wpf.Converters;

public class MultiValueConverter<T1, T2, TTarget> : MultiValueConverterBase<TTarget>
{
    private static readonly Type[] GenericArguments = {
        typeof (T1),
        typeof (T2),
        typeof (TTarget)
    };

    public MultiValueConverter() : base(GenericArguments)
    {
    }

    protected sealed override TTarget? ConvertCore(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(CheckValue<T1>(values, 0, nameof(ConvertCore)), CheckValue<T2>(values, 1, nameof(ConvertCore)), parameter, culture);
    }

    protected sealed override object?[] ConvertBackCore(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        ConvertBack((TTarget)value, out var obj1, out var obj2, parameter, culture);
        return new object?[] { obj1, obj2 };
    }

    protected virtual TTarget? Convert(T1? value1, T2? value2, object parameter, CultureInfo culture)
    {
        throw this.MakeConverterFunctionNotDefinedException(nameof(Convert));
    }

    protected virtual void ConvertBack(TTarget value, out T1? value1, out T2? value2, object parameter, CultureInfo culture)
    {
        throw this.MakeConverterFunctionNotDefinedException(nameof(ConvertBack));
    }
}

public class MultiValueConverter<T1, T2, T3, TTarget> : MultiValueConverterBase<TTarget>
{
    private static readonly Type[] GenericArguments = {
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (TTarget)
    };

    public MultiValueConverter() : base(GenericArguments)
    {
    }

    protected sealed override TTarget? ConvertCore(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(CheckValue<T1>(values, 0, nameof(ConvertCore)), CheckValue<T2>(values, 1, nameof(ConvertCore)),
            CheckValue<T3>(values, 2, nameof(ConvertCore)), parameter, culture);
    }

    protected sealed override object?[] ConvertBackCore(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        ConvertBack((TTarget)value, out var obj1, out var obj2, out var obj3, parameter, culture);
        return new object?[]
        {
            obj1,
            obj2,
            obj3
        };
    }

    protected virtual TTarget? Convert(T1? value1, T2? value2, T3? value3, object parameter, CultureInfo culture)
    {
        throw this.MakeConverterFunctionNotDefinedException(nameof(Convert));
    }

    protected virtual void ConvertBack(TTarget value, out T1 value1, out T2 value2, out T3 value3, object parameter, CultureInfo culture)
    {
        throw this.MakeConverterFunctionNotDefinedException(nameof(ConvertBack));
    }
}

public class MultiValueConverter<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> : MultiValueConverterBase<TTarget>
{
    private static readonly Type[] GenericArguments = {
      typeof (T1),
      typeof (T2),
      typeof (T3),
      typeof (T4),
      typeof (T5),
      typeof (T6),
      typeof (T7),
      typeof (T8),
      typeof (T9),
      typeof (TTarget)
    };

    public MultiValueConverter() : base(GenericArguments)
    {
    }

    protected sealed override TTarget? ConvertCore(object?[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(CheckValue<T1>(values, 0, nameof(ConvertCore)), CheckValue<T2>(values, 1, nameof(ConvertCore)), CheckValue<T3>(values, 2, nameof(ConvertCore)), CheckValue<T4>(values, 3, nameof(ConvertCore)), CheckValue<T5>(values, 4, nameof(ConvertCore)), CheckValue<T6>(values, 5, nameof(ConvertCore)), CheckValue<T7>(values, 6, nameof(ConvertCore)), CheckValue<T8>(values, 7, nameof(ConvertCore)), CheckValue<T9>(values, 8, nameof(ConvertCore)), parameter, culture);
    }

    protected sealed override object?[] ConvertBackCore(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        ConvertBack((TTarget)value, out var obj1, out var obj2, out var obj3, out var obj4, out var obj5, out var obj6,
            out var obj7, out var obj8, out var obj9, parameter, culture);
        return new object?[]
        {
            obj1,
            obj2,
            obj3,
            obj4,
            obj5,
            obj6,
            obj7,
            obj8,
            obj9
        };
    }

    protected virtual TTarget? Convert(T1? value1, T2? value2, T3? value3, T4? value4, T5? value5, 
        T6? value6, T7? value7, T8? value8, T9? value9, object parameter, CultureInfo culture)
    {
        throw this.MakeConverterFunctionNotDefinedException(nameof(Convert));
    }

    protected virtual void ConvertBack(TTarget value, out T1 value1, out T2 value2, out T3 value3, out T4 value4, 
        out T5 value5, out T6 value6, out T7 value7, out T8 value8, out T9 value9, object parameter, CultureInfo culture)
    {
        throw this.MakeConverterFunctionNotDefinedException(nameof(ConvertBack));
    }
}