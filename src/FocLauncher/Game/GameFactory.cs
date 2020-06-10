using System;
using System.IO;

namespace FocLauncher.Game
{
    public class GameFactory
    {
        public static IGame CreateFocGame(DirectoryInfo directory, GameType type)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));
            switch (type)
            {
                case GameType.SteamGold:
                    return new SteamGame(directory);
                case GameType.Disk:
                case GameType.Origin:
                case GameType.GoG:
                case GameType.DiskGold:
                    return new Foc(directory, type);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public static IGame CreateEawGame(DirectoryInfo directory, GameType type)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));
            // TODO: Consume type and create SteamGameEaw
            return new Eaw(directory);
        }
    }
}