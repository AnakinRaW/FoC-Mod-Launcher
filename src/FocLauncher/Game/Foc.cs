using System;
using System.IO;
using FocLauncher.Core.Mods;
using FocLauncher.Core.Utilities;

namespace FocLauncher.Core.Game
{
    public class Foc : AbstractFocGame
    {
        public const string GameconstantsUpdateHash = "b0818f73031b7150a839bb83e7aa6187";
        public const string GraphicdetailsUpdateHash = "4d7e140887fc1dd52f47790a6e20b5c5";

        public override GameType Type { get; }

        protected override string GameExeFileName => "swfoc.exe";

        protected override string? DebugGameExeFileName => null;

        protected override int DefaultXmlFileCount => 2;

        public override string Name => "Forces of Corruption";

        public Foc(string gameDirectory, GameType type) : base(gameDirectory)
        {
            Type = type;
        }

        public override bool IsPatched()
        {
            if (!File.Exists(GameDirectory + @"\Data\XML\GAMECONSTANTS.XML") ||
                !File.Exists(GameDirectory + @"\Data\XML\GRAPHICDETAILS.XML"))
                return false;
            var hashProvider = new HashProvider();
            if (hashProvider.GetFileHash(GameDirectory + @"\Data\XML\GAMECONSTANTS.XML") != GameconstantsUpdateHash)
                return false;
            if (hashProvider.GetFileHash(GameDirectory + @"\Data\XML\GRAPHICDETAILS.XML") != GraphicdetailsUpdateHash)
                return false;
            return true;
        }

        protected override void OnGameStarting(IMod mod, ref GameRunArguments args)
        {
            if (!(mod is DummyMod) && !mod.ModDirectory.StartsWith(GameDirectory))
                throw new Exception("Mod is not compatible");

            if (mod is Mod)
                args.ModPath = "Mods/" + mod.FolderName;

        }
    }
}
