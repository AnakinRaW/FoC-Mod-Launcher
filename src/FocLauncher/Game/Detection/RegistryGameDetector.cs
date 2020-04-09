using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher.Threading;
using FocLauncher.Utilities;

namespace FocLauncher.Game.Detection
{
    public class RegistryGameDetector : GameDetector
    {
        private const string SetupMessage =
            "Your games seem to be installed but are not settet up correctly. Please run vanilla Forces of Corruption at least once to finish the setup.\r\n\r\n" +
            "The launcher can open the Steam-Version of the game for you now (and close it immediately after setup).\r\n" +
            "Would you like to setup the games now?";

        protected override GameDetection DetectGamesCore()
        {
            var registryResult = FindGamesFromRegistry();
            Logger.Trace("Registry game detection result:");
            Logger.Trace("\t" + registryResult);
            return registryResult;
        }

        private GameDetection FindGamesFromRegistry()
        {
            Logger.Trace("Atempting to fetch the game from the registry.");
            var eawResult = CheckGameExists(EaWRegistryHelper.Instance);
            var focResult = CheckGameExists(FocRegistryHelper.Instance);

            if (eawResult == DetectionResult.NotInstalled || focResult == DetectionResult.NotInstalled)
            {
                Logger.Trace("The games are not found in the registry");
                return GameDetection.NotInstalled;
            }

            if (eawResult == DetectionResult.NotSettedUp || focResult == DetectionResult.NotSettedUp)
            {
                if (RunSteamInitialization())
                {
                    Logger.Trace("After initialization, the games are now setted up.");
                    return new GameDetection(new FileInfo(EaWRegistryHelper.Instance.ExePath),
                        new FileInfo(FocRegistryHelper.Instance.ExePath));
                }
                Logger.Trace("The games are (still) not setted up.");
                return GameDetection.NotSettedUp;
            }

            if (eawResult == DetectionResult.Installed && focResult == DetectionResult.Installed)
            {
                Logger.Trace("The games have been found in the registry.");
                return new GameDetection(new FileInfo(EaWRegistryHelper.Instance.ExePath),
                    new FileInfo(FocRegistryHelper.Instance.ExePath));
            }
            return GameDetection.NotInstalled;
        }

        private static DetectionResult CheckGameExists(PetroglyphGameRegistry gameRegistry)
        {
            if (!gameRegistry.Exists)
                return DetectionResult.NotInstalled;
            if (!gameRegistry.Installed)
                return DetectionResult.NotSettedUp;
            return DetectionResult.Installed;
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

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                // TODO: Use wait dialog with text update
                await SetupSteamGamesAsync(new CancellationToken());
            });

            Logger.Trace("Re-try checking the game is setted up in the registry.");
            return CheckGameExists(EaWRegistryHelper.Instance) == DetectionResult.Installed && CheckGameExists(FocRegistryHelper.Instance) == DetectionResult.Installed;
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