namespace AnakinRaW.CommonUtilities.Wpf.Utilities;

public static class Boxes
{
    public static readonly object BooleanTrue = true;
    public static readonly object BooleanFalse = false;
    public static readonly object Int32Zero = 0;
    public static readonly object Int32One = 1;
    public static readonly object UInt32Zero = 0U;
    public static readonly object UInt32One = 1U;
    public static readonly object UInt64Zero = 0UL;
    public static readonly object DoubleZero = 0.0;

    public static object Box(bool value) => !value ? BooleanFalse : BooleanTrue;

    public static object? Box(bool? nullableValue) => !nullableValue.HasValue ? null : Box(nullableValue.Value);

    public static object Box(int value)
    {
        if (value == 0)
            return Int32Zero;
        return value == 1 ? Int32One : value;
    }

    public static object Box(uint value)
    {
        if (value == 0U)
            return UInt32Zero;
        return value == 1U ? UInt32One : value;
    }

    public static object Box(ulong value) => value != 0UL ? value : UInt64Zero;

    public static object Box(double value) => value != 0.0 ? value : DoubleZero;
}