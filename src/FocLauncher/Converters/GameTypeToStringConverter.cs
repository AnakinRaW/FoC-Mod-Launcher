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
            GameType gameType;
            if (value is IGame game)
                gameType = game.Type;
            else if (value is GameType type)
                gameType = type;
            else
                return "Game not found";

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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
