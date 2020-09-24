using System;
using System.Globalization;
using System.Windows.Data;

namespace FocLauncher.Converters
{
    internal sealed class IsNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
                return string.IsNullOrEmpty(text);
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
