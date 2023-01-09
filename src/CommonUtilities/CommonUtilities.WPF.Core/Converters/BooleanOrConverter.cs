using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace AnakinRaW.CommonUtilities.Wpf.Converters;

public class BooleanOrConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValues = values.OfType<bool>();
        return boolValues.Any(_ => true);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}