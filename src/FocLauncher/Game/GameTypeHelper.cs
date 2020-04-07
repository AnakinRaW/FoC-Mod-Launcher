using System.IO;

namespace FocLauncher.Game
{
    public static class GameTypeHelper
    {
        // TODO: Test all checks again 
        internal static GameType GetGameType(ref GameDetectionResult result)
        {
            if (CheckSteam(result.FocPath))
                return GameType.SteamGold;
            if (CheckGoG(result.FocPath))
                return GameType.GoG;
            if (CheckOrigin(ref result))
                return GameType.Origin;
            return GameType.Disk;
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