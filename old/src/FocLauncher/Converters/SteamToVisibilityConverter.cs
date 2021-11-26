using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FocLauncher.Game;

namespace FocLauncher.Converters
{
    internal class SteamToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SteamGameFoc)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
