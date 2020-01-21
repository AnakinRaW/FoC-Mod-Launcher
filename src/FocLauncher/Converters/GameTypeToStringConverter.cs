using System;
using System.Globalization;
using System.Windows.Data;
using FocLauncher.Game;

namespace FocLauncher.Converters
{
    internal class GameTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameType gameType)
            {
                switch (gameType)
                {
                    case GameType.Disk:
                        return "Disk Version";
                    case GameType.DiskGold:
                        return "Disk Gold Version";
                    case GameType.SteamGold:
                        return "Steam Version";
                    case GameType.GoG:
                        return "GoG Version";
                    case GameType.Origin:
                        return "Origin Version";
                    default:
                        return "FoC was not found";
                }
            }
            return "FoC was not found";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
