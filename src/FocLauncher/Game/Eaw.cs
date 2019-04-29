using System;
using System.IO;
using FocLauncher.Core.Mods;
using FocLauncher.Core.Utilities;

namespace FocLauncher.Core.Game
{
    public class Eaw : IGame
    {
        public const string GameconstantsUpdateHashEaW = "1d44b0797c8becbe240adc0098c2302a";

        public string GameDirectory { get; protected set; }
        public string Name => "Empire at War";
        public GameProcessData GameProcessData => new GameProcessData();

        public Eaw(string gameDirectory)
        {
            GameDirectory = gameDirectory;
            if (!Exists())
                throw new Exception("EaW does not exists");
        }

        public bool Exists() => Directory.Exists(GameDirectory) && File.Exists(Path.Combine(GameDirectory, "sweaw.exe"));

        public void PlayGame()
        {
        }

        public void PlayGame(IMod mod, DebugOptions debugOptions)
        {
        }

        public bool IsPatched()
        {
            if (!File.Exists(Path.Combine(GameDirectory, @"Data\XML\GAMECONSTANTS.xml")))
                return false;
            var hashProvider = new HashProvider();
            if (hashProvider.GetFileHash(Path.Combine(GameDirectory, @"Data\XML\GAMECONSTANTS.xml")) != GameconstantsUpdateHashEaW)
                return false;
            if (Directory.GetFiles(Path.Combine(GameDirectory, @"Data\XML\")).Length != 1)
                return false;
            return true;
        }

        public bool IsGameAiClear()
        {
            throw new NotImplementedException();
        }

        public static bool FindInstallationRelativeToFoc(string focPath, GameType type, out string eawPath)
        {
            eawPath = string.Empty;
            switch (type)
            {
                case GameType.Disk:
                    if (!File.Exists(Path.Combine(Directory.GetParent(focPath).FullName, @"Star Wars Empire at War\GameData\sweaw.exe")))
                        return false;
                    eawPath = Path.Combine(Directory.GetParent(focPath).FullName, @"Star Wars Empire at War\GameData\");
                    break;
                case GameType.SteamGold:
                case GameType.GoG:
                    if (!File.Exists(Path.Combine(Directory.GetParent(focPath).FullName, "GameData\\sweaw.exe")))
                        return false;
                    eawPath = Path.Combine(Directory.GetParent(focPath).FullName, "GameData\\");
                    break;
                case GameType.DiskGold:
                case GameType.Undefined:
                    return false;
            }
            return true;
        }
    }
}
