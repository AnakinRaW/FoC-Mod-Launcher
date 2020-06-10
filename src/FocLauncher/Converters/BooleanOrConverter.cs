using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace FocLauncher.Converters
{
    public class BooleanOrConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValues = values.OfType<bool>();
            return boolValues.Any(x => true);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
