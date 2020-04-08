using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher.Threading;
using FocLauncher.Utilities;
using NLog;

namespace FocLauncher.Game
{
    public static class GameDetectionHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string SetupMessage =
            "Your games seem to be installed but are not settet up correctly. Please run vanilla Forces of Corruption at least once to finish the setup.\r\n\r\n" +
            "The launcher can open the Steam-Version of the game for you now (and close it immediately after setup).\r\n" +
            "Would you like to setup the games now?";

        internal static GameDetectionResult GetGameInstallations()
        {
            var result = FindGamesFromRegistry();
            if (result.IsError)
                return result;


            // TODO: Check from here
            result.FocType = GameTypeHelper.GetGameType(result);

            FindGamesFromExecutingPath(result);

            if (string.IsNullOrEmpty(result.FocExePath) || !File.Exists(result.FocExePath))
            {
                result.Error = DetectionError.NotInstalled;
                return result;
            }
            result.FocType = GameTypeHelper.GetGameType(result);
            return result;
        }

        private static GameDetectionResult FindGamesFromRegistry()
        {
            Logger.Trace("Atempting to fetch the game from the registry.");
            var eawResult = CheckGameExists(EaWRegistryHelper.Instance);
            var focResult = CheckGameExists(FocRegistryHelper.Instance);

            var result = new GameDetectionResult();

            if (eawResult == DetectionError.NotInstalled || focResult == DetectionError.NotInstalled)
            {
                Logger.Trace("The games are not found in the registry");
                result.Error = DetectionError.NotInstalled;
                return result;
            }

            if (eawResult == DetectionError.NotSettedUp || focResult == DetectionError.NotSettedUp)
            {
                if (RunSteamInitialization())
                {
                    Logger.Trace("After initialization, the games are now setted up.");
                    result.EawExePath = EaWRegistryHelper.Instance.ExePath;
                    result.FocExePath = FocRegistryHelper.Instance.ExePath;
                    return result;
                }
                Logger.Trace("The games are (still) not setted up.");
                result.Error = DetectionError.NotSettedUp;
                return result;
            }

            if (eawResult == DetectionError.None && focResult == DetectionError.None)
            {
                Logger.Trace("The games have been found in the registry.");
                result.EawExePath = EaWRegistryHelper.Instance.ExePath;
                result.FocExePath = FocRegistryHelper.Instance.ExePath;
                return result;
            }
            return result;
        }
        
        private static void FindGamesFromExecutingPath(GameDetectionResult result)
        {
            // TODO
            //var currentPath = Directory.GetCurrentDirectory();

            //if (!File.Exists(Path.Combine(currentPath, "swfoc.exe")))
            //    return;

            //if (Path.GetFullPath(currentPath) == Path.GetFullPath(result.FocPath))
            //    return;

            //var newResult = default(GameDetectionResult);
            //newResult.FocPath = currentPath;

            //var gameType = GameTypeHelper.GetGameType(newResult);
            //newResult.FocType = gameType;
            //if (!Eaw.FindInstallationRelativeToFoc(newResult.FocPath, gameType, out var eawPath))
            //{
            //    newResult.EawPath = result.EawPath;
            //    result = newResult;
            //    return;
            //}
            //newResult.EawPath = eawPath;
            //result = newResult;
        }

        private static DetectionError CheckGameExists(PetroglyphGameRegistry gameRegistry)
        {
            if (!gameRegistry.Exists)
                return DetectionError.NotInstalled;
            if (!gameRegistry.Installed)
                return DetectionError.NotSettedUp;
            return DetectionError.None;
        }

        private static bool RunSteamInitialization()
        {
            Logger.Trace("The games are not setted up. Trying to set them up by running the game once (Steam only)");
            var steamClient = SteamClient.Instance;
            if (!steamClient.Installed || !SteamClient.Instance.IsGameInstalled(SteamGame.EmpireAtWarSteamId) && 
                !SteamClient.Instance.IsGameInstalled(SteamGame.ForcesOfCorruptionSteamId))
                return false;
            Logger.Trace("Steam and the games are installed. Asing the user whether to run setup now.");
            if (!PromptGameSetupDialog())
            {
                Logger.Warn("User denied steam setup.");
                return false;
            }

            ThreadHelper.JoinableTaskFactory.Run(async ()=>
            {
                // TODO: Use wait dialog with text update
                await SetupSteamGamesAsync(new CancellationToken());
            });

            Logger.Trace("Re-try checking the game is setted up in the registry.");
            return CheckGameExists(EaWRegistryHelper.Instance) == DetectionError.None && CheckGameExists(FocRegistryHelper.Instance) == DetectionError.None;
        }

        private static async Task SetupSteamGamesAsync(CancellationToken cancellationToken)
        {
            Logger.Trace("Atempting to configure the Steam Version");
            var eawSlim = new EawSteamGameSlim();
            try
            {
                // TODO: Check steam running and await start if not
                eawSlim.Close();
                eawSlim.StartGame();
                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                linkedTokenSource.CancelAfter(30_000);
                Logger.Trace("Waiting max. 30 seconds for the game to be started");
                await ProcessCreationListener.WaitProcessCreatedAsync(EawSteamGameSlim.Executable, linkedTokenSource.Token);
                if (!linkedTokenSource.IsCancellationRequested)
                {
                    Logger.Trace($"{EawSteamGameSlim.Executable} was started. Waiting another second. Just to be sure");
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Trace("The procedure was cancelled or reached the timeout");
            }
            finally
            {
                Logger.Trace("Close the application again if it was started.");
                eawSlim.Close();
            }
        }

        internal static bool PromptGameSetupDialog()
        {
            var mbResult = MessageBox.Show(SetupMessage, "FoC Launcher", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
            return mbResult == MessageBoxResult.Yes;
        }
    }
}
