using System;
using System.Diagnostics;
using System.IO;
using FocLauncher.Mods;
using FocLauncher.Utilities;
using FocLauncher.Versioning;

namespace FocLauncher.Game
{
    public class Foc : AbstractFocGame
    {
        public const string GameconstantsUpdateHash = "b0818f73031b7150a839bb83e7aa6187";
        public const string GraphicdetailsUpdateHash = "4d7e140887fc1dd52f47790a6e20b5c5";

        protected override string GameExeFileName => "swfoc.exe";

        protected override int DefaultXmlFileCount => 2;

        public override string Name => "Forces of Corruption";

        public Foc() { }

        public Foc(string gameDirectory) : base(gameDirectory)
        {
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

        public override void PlayGame()
        {
            PlayGame(null, default);
        }

        public override void PlayGame(IMod mod, DebugOptions debugOptions)
        {
            if (!mod.ModDirectory.StartsWith(GameDirectory))
                throw new Exception("Mod is not compatible");

            string args;
            if (mod is Mod)
                args = "MODPATH=" + "Mods/" + mod.FolderName;
            else
                args = string.Empty;

            var process = new Process
            {
                StartInfo =
                {
                    FileName = GameDirectory + @"\swfoc.exe",
                    Arguments = args,
                    WorkingDirectory = GameDirectory,
                    UseShellExecute = false
                }
            };
            try
            {
                GameStartHelper.StartGameProcess(process, mod.IconFile);
            }
            catch (Exception)
            {
                //ignored
            }
        }
    }
}
