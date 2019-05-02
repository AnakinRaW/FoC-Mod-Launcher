using System.Diagnostics;
using System.IO;
using System.Windows;
using FocLauncher.Core.Utilities;
using Microsoft.Win32;

namespace FocLauncher.Core.Game
{
    public static class GameHelper
    {
        internal const string EawRegistryPath = @"SOFTWARE\LucasArts\Star Wars Empire at War";
        internal const string FocRegistryPath = @"SOFTWARE\LucasArts\Star Wars Empire at War Forces of Corruption";
        internal const string FocRegistryVersion = @"\1.0";
        internal const string EawRegistryVersion = @"\1.0";

        private const string SetupMessage =
            "Your games seem to be installed but are not settet up correctly. Please run vanilla Forces of Corruption at least once to finish the setup.\r\n\r\n" +
            "The launcher can open the Steam-Version of the game for you now (and close it immediately after setup).\r\n" +
            "Would you like to setup the games now?";

        internal static GameDetectionResult GetGameInstallations()
        {
            var result = default(GameDetectionResult);

            FindGamesFromRegistry(ref result);
            if (result.IsError)
                return result;

            result.FocType = GetGameType(ref result);

            FindGamesFromExecutingPath(ref result);

            if (string.IsNullOrEmpty(result.FocPath) || !File.Exists(Path.Combine(result.FocPath + "\\swfoc.exe")))
            {
                result.IsError = true;
                result.Error = DetectionError.NotInstalled;
                return result;
            }
            result.FocType = GetGameType(ref result);
            return result;
        }

        private static GameType GetGameType(ref GameDetectionResult result)
        {
            if (CheckSteam(result.FocPath))
                return GameType.SteamGold;
            if (CheckGoG(result.FocPath))
                return GameType.GoG;
            if (CheckOrigin(ref result))
                return GameType.Origin;
            return GameType.Disk;
        }

        private static void FindGamesFromRegistry(ref GameDetectionResult result)
        {
            var eawResult = CheckGameExists(EawRegistryPath, EawRegistryVersion);
            var focResult = CheckGameExists(FocRegistryPath, FocRegistryVersion);

            if (eawResult == DetectionError.None && focResult == DetectionError.None)
            {
                result.EawPath = GetGamePathFromRegistry(EawRegistryPath + EawRegistryVersion);
                result.FocPath = GetGamePathFromRegistry(FocRegistryPath + FocRegistryVersion);
                return;
            }
            if (eawResult == DetectionError.NotInstalled || focResult == DetectionError.NotInstalled)
            {
                result.IsError = true;
                result.Error = DetectionError.NotInstalled;
                return;
            }
            if (eawResult == DetectionError.NotSettedUp || focResult == DetectionError.NotSettedUp)
            {
                if (Steam.IsSteamGoldPackInstalled() && PromptGameSetupDialog() && SetupSteamGames())
                {
                    result.EawPath = GetGamePathFromRegistry(EawRegistryPath + EawRegistryVersion);
                    result.FocPath = GetGamePathFromRegistry(FocRegistryPath + FocRegistryVersion);
                    return;
                }
                result.IsError = true;
                result.Error = DetectionError.NotSettedUp;
            }
        }

        private static void FindGamesFromExecutingPath(ref GameDetectionResult result)
        {
            var currentPath = Directory.GetCurrentDirectory();

            if (!File.Exists(Path.Combine(currentPath, "swfoc.exe")))
                return;

            if (result.FocPath.NormalizePath() == currentPath.NormalizePath())
                return;

            var newResult = default(GameDetectionResult);
            newResult.FocPath = currentPath;

            var gameType = GetGameType(ref newResult);
            newResult.FocType = gameType;
            if (!Eaw.FindInstallationRelativeToFoc(newResult.FocPath, gameType, out var eawPath))
            {
                newResult.EawPath = result.EawPath;
                result = newResult;
                return;
            }
            newResult.EawPath = eawPath;
            result = newResult;
        }

        private static DetectionError CheckGameExists(string baseKeyPath, string versionKeyPath)
        {
            using (var registry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                var key = registry.OpenSubKey(baseKeyPath + versionKeyPath, false);
                if (key == null)
                {
                    var baseKey = registry.OpenSubKey(baseKeyPath, false);
                    return baseKey == null ? DetectionError.NotInstalled : DetectionError.NotSettedUp;
                }
                var installed = (int)key.GetValue("installed");
                return installed == 0 ? DetectionError.NotInstalled : DetectionError.None;
            }
        }

        private static string GetGamePathFromRegistry(string registryPath)
        {
            using (var registry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                var exePath = (string)registry.OpenSubKey(registryPath, false)?.GetValue("exepath");
                return new FileInfo(exePath).Directory.FullName;
            }
        }

        private static bool SetupSteamGames()
        {
            ProcessHelper.FindProcess("StarWarsG")?.Kill();
            Process.Start("steam://rungameid/32470");

            for (var count = 0; count <= 5000; count++)
            {
                var eaw = ProcessHelper.FindProcess("StarWarsG");
                if (eaw != null)
                {
                    if (CheckGameExists(EawRegistryPath, EawRegistryVersion) == DetectionError.None &&
                        CheckGameExists(FocRegistryPath, FocRegistryVersion) == DetectionError.None)
                    {
                        eaw.Kill();
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool PromptGameSetupDialog()
        {
            var mbResult = MessageBox.Show(SetupMessage, "FoC Launcher", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
            return mbResult == MessageBoxResult.Yes;
        }

        private static bool CheckSteam(string path)
        {
            if (new DirectoryInfo(path).Name != "corruption")
                return false;
            if (!Directory.Exists(Directory.GetParent(path).FullName + "\\GameData\\"))
                return false;
            if (!File.Exists(Directory.GetParent(path) + "\\GameData\\sweaw.exe"))
                return false;
            if (!File.Exists(Directory.GetParent(path) + "\\runm2.dat") ||
                !File.Exists(Directory.GetParent(path) + "\\runme.dat"))
                return false;
            return true;
        }

        private static bool CheckGoG(string path)
        {
            if (new DirectoryInfo(path).Name != "EAWX")
                return false;
            if (!File.Exists(Directory.GetParent(path) + "\\GameData\\sweaw.exe"))
                return false;
            if (!File.Exists(Directory.GetParent(path) + "\\GameData\\goggame-1421404887.dll"))
                return false;
            return true;
        }

        private static bool CheckOrigin(ref GameDetectionResult result)
        {
            FixPossibleOriginBug(ref result);
            if (new DirectoryInfo(result.FocPath).Name != "EAWX")
                return false;
            if (!Directory.Exists(Path.Combine(Directory.GetParent(result.FocPath).FullName, "Manuals")))
                return false;
            if (!Directory.Exists(Path.Combine(Directory.GetParent(result.FocPath).FullName, "__Installer")))
                return false;
            return true;
        }


        private static void FixPossibleOriginBug(ref GameDetectionResult result)
        {
            var exeDir = new DirectoryInfo(result.FocPath);
            if (exeDir.Name == "corruption")
            {
                var parentPath = exeDir.Parent?.FullName;
                if (parentPath == null)
                    return;

                var correctedPath = Path.Combine(parentPath, "EAWX");
                if (Directory.Exists(correctedPath))
                    result.FocPath = correctedPath;
            }
        }
    }
}
