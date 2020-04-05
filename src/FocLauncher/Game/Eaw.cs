using System;
using System.IO;
using FocLauncher.Utilities;

namespace FocLauncher.Game
{
    public class Eaw : AbstractFocGame
    {
        public const string GameconstantsUpdateHashEaW = "1d44b0797c8becbe240adc0098c2302a";

        public Eaw(string gameDirectory) : base(gameDirectory)
        {
        }

        public override GameType Type => GameType.Undefined;
        public override string Name => "Star Wars: Empire at War";
        protected override int DefaultXmlFileCount => 1;
        protected override string GameExeFileName => "sweaw.exe";
        protected override string? DebugGameExeFileName => null;

        public override bool IsPatched()
        {
            var constantsFilePath = Path.Combine(GameDirectory, @"Data\XML\GAMECONSTANTS.xml");
            if (!File.Exists(constantsFilePath))
                return false;
            var hashProvider = new HashProvider();
            if (hashProvider.GetFileHash(constantsFilePath) != GameconstantsUpdateHashEaW)
                return false;
            return Directory.GetFiles(Path.Combine(GameDirectory, @"Data\XML\")).Length == 1;
        }

        public override bool IsGameAiClear()
        {
            throw new NotSupportedException();
        }

        public override bool IsLanguageInstalled(string language)
        {
            return false;
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
