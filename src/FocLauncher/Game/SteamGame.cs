using System;
using System.Diagnostics;
using System.IO;
using FocLauncher.Core.Mods;
using FocLauncher.Core.Utilities;

namespace FocLauncher.Core.Game
{
    public sealed class SteamGame : AbstractFocGame
    {
        public const string GameconstantsUpdateHash = "b0818f73031b7150a839bb83e7aa6187";

        protected override string GameExeFileName => "StarwarsG.exe";
        protected string DebugGameExeFileName => "StarwarsI.exe";

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

        public override void PlayGame()
        {
            PlayGame(null, default);
        }

        public override void PlayGame(IMod mod, DebugOptions debugOptions)
        {
            if (!Exists())
                throw new Exception("FoC was not found");

            if (!Steam.IsSteamRunning())
                Steam.StartSteam();

            string arguments;
            if (mod is DummyMod)
                arguments = string.Empty;
            else
            {
                if (!mod.WorkshopMod)
                    arguments = "MODPATH=" + "Mods/" + mod.FolderName;
                else
                    arguments = "STEAMMOD=" + mod.FolderName;
            }

            if (debugOptions.UseDebug)
            {
                if (debugOptions.IgnoreAsserts)
                    arguments += arguments + " IGNOREASSERTS";
                if (debugOptions.NoArtProcess)
                    arguments += arguments + " NOARTPROCESS";
            }

            var exePath = Path.Combine(GameDirectory, debugOptions.UseDebug ? DebugGameExeFileName : GameExeFileName);

            var process = new Process
            {
                StartInfo =
                {
                    FileName = exePath,
                    Arguments = arguments,
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

        public bool HasDebugBuild()
        {
            return File.Exists(Path.Combine(GameDirectory, DebugGameExeFileName));
        }
    }
}
