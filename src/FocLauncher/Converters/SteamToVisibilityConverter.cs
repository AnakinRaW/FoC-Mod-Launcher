using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FocLauncher.Core.Game;

namespace FocLauncher.Core.Converters
{
    internal class SteamToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IGame game && game.HasDebugBuild())
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
