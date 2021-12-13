using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sklavenwalker.CommonUtilities.Wpf.Utils;

internal static class ConverterExceptionHelpers
{
    public static Exception MakeValueNotOfTypeException(this object converter, Type expectedValueType, Type actualValueType, 
        string paramName, [CallerMemberName] string caller = "")
    {
        var str = actualValueType.FullName ?? "(unknown)";
        return new ArgumentException(
            $"{converter.GetType().FullName}.{caller}: unexpected value type:{Environment.NewLine}Expected type: {expectedValueType.FullName}{Environment.NewLine}Actual type: {str}", paramName);
    }

    public static Exception MakeTargetNotExtendingTypeException(this object converter, Type expectedTargetType, 
        Type actualTargetType, [CallerMemberName] string caller = "")
    {
        var str = actualTargetType?.FullName ?? "(unknown)";
        return new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture,
            "{1}.{2}: unexpected target type:{0}Expected target type: {3}{0}Actual target type: {4}", Environment.NewLine, converter.GetType().FullName, caller,
            expectedTargetType.FullName, str));
    }

    public static Exception MakeConverterFunctionNotDefinedException(this object converter, [CallerMemberName] string caller = "")
    {
        return new NotSupportedException(string.Format(CultureInfo.CurrentUICulture,
            "{0}: {1} is not defined for this converter.", converter.GetType().FullName, caller));
    }
}