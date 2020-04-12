using System;
using System.IO;
using FocLauncher.Mods;
using FocLauncher.Utilities;

namespace FocLauncher.Game
{
    public class Foc : PetroglyphGame
    {
        public const string GameconstantsUpdateHash = "b0818f73031b7150a839bb83e7aa6187";
        public const string GraphicdetailsUpdateHash = "4d7e140887fc1dd52f47790a6e20b5c5";

        public override GameType Type { get; }

        protected override string GameExeFileName => "swfoc.exe";

        protected override string? DebugGameExeFileName => null;

        protected override int DefaultXmlFileCount => 2;

        public override string Name => "Forces of Corruption";
        public override string Description => string.Empty;

        public override string? IconFile => LauncherDataModel.IconPath;

        public Foc(string gameDirectory, GameType type) : base(gameDirectory)
        {
            Type = type;
        }

        public override bool IsPatched()
        {
            var constantsFilePath = Path.Combine(GameDirectory, @"Data\XML\GAMECONSTANTS.XML");
            var graphicsFilePath = Path.Combine(GameDirectory, @"Data\XML\GRAPHICDETAILS.XML");
            if (!File.Exists(constantsFilePath) || !File.Exists(graphicsFilePath))
                return false;
            var hashProvider = new HashProvider();
            if (hashProvider.GetFileHash(constantsFilePath) != GameconstantsUpdateHash)
                return false;
            return hashProvider.GetFileHash(graphicsFilePath) == GraphicdetailsUpdateHash;
        }

        protected override void OnGameStarting(GameStartingEventArgs args)
        {
            if (!(args.GameArguments.Mod is DummyMod) && !args.GameArguments.Mod.ModDirectory.StartsWith(GameDirectory))
                throw new Exception("Mod is not compatible");
            base.OnGameStarting(args);
        }
    }
}
