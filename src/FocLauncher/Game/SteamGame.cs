using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FocLauncher.Mods;
using FocLauncher.Threading;
using FocLauncher.Utilities;
using FocLauncher.WaitDialog;

namespace FocLauncher.Game
{
    public sealed class SteamGame : PetroglyphGame, IDebugable
    {
        private const string DebugGameExeFileName = "StarwarsI.exe";

        public const string GameconstantsUpdateHash = "b0818f73031b7150a839bb83e7aa6187";

        public const int EmpireAtWarSteamId = 32470;
        public const int ForcesOfCorruptionSteamId = 32472;

        protected override string GameExeFileName => "StarwarsG.exe";

        protected override int DefaultXmlFileCount => 1;

        public override GameType Type => GameType.SteamGold;

        public override string Name => "Forces of Corruption (Steam)";

        public override string? IconFile => LauncherApp.IconPath;

        public override string Description => string.Empty;

        public bool DebugBuildExists => File.Exists(Path.Combine(Directory.FullName, DebugGameExeFileName));

        public SteamGame(DirectoryInfo gameDirectory) : base(gameDirectory)
        {
        }

        public override bool IsPatched()
        {
            var gameConstantsFilePath = Path.Combine(Directory.FullName, @"Data\XML\GAMECONSTANTS.XML");
            if (!File.Exists(gameConstantsFilePath))
                return false;
            var hashProvider = new HashProvider();
            return hashProvider.GetFileHash(gameConstantsFilePath) == GameconstantsUpdateHash;
        }

        protected override void OnGameStarting(GameStartingEventArgs args)
        {
            if (!SteamClient.Instance.IsRunning)
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    var data = new WaitDialogProgressData("Waiting for Steam...", isCancelable: true);
                    using var s = WaitDialogFactory.Instance.StartWaitDialog("FoC Launcher", data, TimeSpan.FromSeconds(2));
                    SteamClient.Instance.StartSteam();
                    try
                    {
                        await SteamClient.Instance.WaitSteamRunningAndLoggedInAsync(s.UserCancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        args.Cancel = true;
                    }
                });
            }
            base.OnGameStarting(args);
        }

        public bool DebugGame(string? iconFile = null)
        {
            return DebugGame(new GameCommandArguments(), iconFile);
        }

        public bool DebugGame(GameCommandArguments args, string? iconFile = null)
        {
            GameStartInfo startInfo;
            if (DebugBuildExists)
            {
                var exeFile = new FileInfo(Path.Combine(Directory.FullName, DebugGameExeFileName));
                startInfo = new GameStartInfo(exeFile, GameBuildType.Debug);
            }
            else
            {
                var exeFile = new FileInfo(Path.Combine(Directory.FullName, GameExeFileName));
                startInfo = new GameStartInfo(exeFile, GameBuildType.Release);
            }
            return StartGame(args, startInfo, iconFile);
        }

        protected override ICollection<IMod> SearchModsCore()
        {
            return SearchSteamMods().ToList();
        }

        private IEnumerable<IMod> SearchSteamMods()
        {
            var mods = new List<IMod>();
            mods.AddRange(SearchDiskMods());

            var workshopsPath = FileUtilities.NormalizePath(Path.Combine(Directory.FullName, @"..\..\..\workshop\content\32470\"));
            var workshopsDir = new DirectoryInfo(workshopsPath);
            if (!workshopsDir.Exists)
                return mods;

            var modDirs = workshopsDir.EnumerateDirectories();
            var workshopMods = modDirs.Select(modDir => ModFactory.CreateMod(this, ModType.Workshops, modDir, true));
            return mods.Union(workshopMods);
        }
    }
}
