using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sklavenwalker.CommonUtilities.Wpf.Utilities;

internal static class ConverterExceptionHelpers
{
    private const string UnknownTypeName = "(unknown)";

    public static Exception MakeValueNotOfTypeException(
        this object converter, Type expectedValueType, Type? actualValueType, string paramName, [CallerMemberName] string caller = "")
    {
        var str = actualValueType?.FullName ?? UnknownTypeName;
        return new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
                "{1}.{2}: unexpected value type:{0}Expected type: {3}{0}Actual type: {4}", Environment.NewLine,
                converter.GetType().FullName, caller, expectedValueType.FullName, str), paramName);
    }

    public static Exception MakeValueAtOffsetNotOfTypeException(
        this object converter, Type expectedValueType, Type? actualValueType, string paramName, int offset, [CallerMemberName] string caller = "")
    {
        var str = actualValueType?.FullName ?? UnknownTypeName;
        return new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
                "{1}.{2}: unexpected value type at offset {5}:{0}Expected type: {3}{0}Actual type: {4}",
                Environment.NewLine, converter.GetType().FullName, caller, expectedValueType.FullName, str, offset),
            paramName);
    }

    public static Exception MakeTargetNotExtendingTypeException(this object converter, Type expectedTargetType, Type actualTargetType, [CallerMemberName] string caller = "")
    {
        var str = actualTargetType?.FullName ?? UnknownTypeName;
        return new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture,
            "{1}.{2}: unexpected target type:{0}Expected target type: {3}{0}Actual target type: {4}",
            Environment.NewLine, converter.GetType().FullName, caller, expectedTargetType.FullName, str));
    }

    public static Exception MakeTargetAtOffsetNotExtendingTypeException(this object converter, Type expectedTargetType, Type actualTargetType, int offset, [CallerMemberName] string caller = "")
    {
        var str = actualTargetType?.FullName ?? UnknownTypeName;
        return new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture,
            "{1}.{2}: unexpected target type at offset {5}:{0}Expected target type: {3}{0}Actual target type: {4}",
            Environment.NewLine, converter.GetType().FullName, caller, expectedTargetType.FullName, str, offset));
    }

    public static Exception MakeConverterFunctionNotDefinedException(this object converter, [CallerMemberName] string caller = "")
    {
        return new NotSupportedException(string.Format(CultureInfo.CurrentUICulture,
            "{0}: {1} is not defined for this converter.", converter.GetType().FullName, caller));
    }

    public static Exception MakeInsufficientSourceParametersException(this object converter, int expectedCount, int actualCount, [CallerMemberName] string caller = "")
    {
        return new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
            "{0}.{1}: Converter requires {2} source parameters, {3} source parameters provided.",
            converter.GetType().FullName, caller, expectedCount, actualCount));
    }

    public static Exception MakeInsufficientTypeParametersException(this object converter, int expectedCount, int actualCount, [CallerMemberName] string caller = "")
    {
        return new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
            "{0}.{1}: Converter requires {2} type parameters, {3} type parameters provided.",
            converter.GetType().FullName, caller, expectedCount, actualCount));
    }
}