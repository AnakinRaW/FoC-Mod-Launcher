using System.Globalization;
using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Converters;

public class BooleanToFlowDirectionConverter : ValueConverter<bool, FlowDirection>
{
    protected override FlowDirection Convert(bool value, object? parameter, CultureInfo culture)
    {
        var flowDirection = FlowDirection.LeftToRight;
        if (value)
            flowDirection = FlowDirection.RightToLeft;
        return flowDirection;
    }
}

public class InverseBooleanConverter : ValueConverter<bool, bool>
{
    protected override bool Convert(bool value, object? parameter, CultureInfo culture)
    {
        return !value;
    }
}