using System.IO;
using FocLauncher.Core.Mods;
using FocLauncher.Core.Utilities;

namespace FocLauncher.Core.Game
{
    public sealed class SteamGame : AbstractFocGame
    {
        public const string GameconstantsUpdateHash = "b0818f73031b7150a839bb83e7aa6187";

        protected override string GameExeFileName => "StarwarsG.exe";
        protected override string DebugGameExeFileName => "StarwarsI.exe";

        protected override int DefaultXmlFileCount => 1;

        public override string Name => "Forces of Corruption (Steam)";

        public SteamGame(string gameDirectory) : base(gameDirectory)
        {
        }

        public override bool IsPatched()
        {
            if (!File.Exists(GameDirectory + @"\Data\XML\GAMECONSTANTS.XML"))
                return false;
            var hashProvider = new HashProvider();
            if (hashProvider.GetFileHash(GameDirectory + @"\Data\XML\GAMECONSTANTS.XML") != GameconstantsUpdateHash)
                return false;
            return true;
        }

        protected override void OnGameStarting(IMod mod, ref GameRunArguments args)
        {
            if (!Steam.IsSteamRunning())
                Steam.StartSteam();
            if (mod != null)
            {
                args.IsWorkshopMod = mod.WorkshopMod;
                if (!args.IsWorkshopMod)
                    args.ModPath = "MODPATH=" + "Mods/" + mod.FolderName;
                else
                    args.SteamMod = mod.FolderName;
            }
        }

        public override bool HasDebugBuild()
        {
            return File.Exists(Path.Combine(GameDirectory, DebugGameExeFileName));
        }
    }
}
