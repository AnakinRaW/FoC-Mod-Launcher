using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FocLauncher.Converters
{
    internal class LauncherItemMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int depth))
                return new Thickness(0);
            return new Thickness(4 + depth * 18, 0,0,0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
