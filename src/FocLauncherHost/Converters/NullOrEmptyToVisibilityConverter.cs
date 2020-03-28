using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FocLauncherHost.Converters
{
    internal class NullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string text))
                return Visibility.Collapsed;
            return string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}