using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FocLauncher.Game;
using FocLauncher.Items;
using FocLauncher.Mods;

namespace FocLauncher.Converters
{
    internal class DebugBuildToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IGame game;
            if (value is LauncherItem launcherItem)
                value = launcherItem.GameObject;
            switch (value)
            {
                case IMod mod:
                    game = mod.Game;
                    break;
                case IGame game1:
                    game = game1;
                    break;
                default:
                    return Visibility.Collapsed;
            }
            if (game is IDebugable debugable && debugable.DebugBuildExists)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}