using System.IO;

namespace FocLauncher.Game
{
    public static class GameTypeHelper
    {
        // TODO: Test all checks again 
        internal static GameType GetGameType(GameDetection result)
        {
            if (CheckSteam(result.FocExePath))
                return GameType.SteamGold;
            if (CheckGoG(result.FocExePath))
                return GameType.GoG;
            if (CheckOrigin(result))
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

        private static bool CheckOrigin(GameDetection result)
        {
            FixPossibleOriginBug(result);
            if (new DirectoryInfo(result.FocExePath).Name != "EAWX")
                return false;
            if (!Directory.Exists(Path.Combine(Directory.GetParent(result.FocExePath).FullName, "Manuals")))
                return false;
            if (!Directory.Exists(Path.Combine(Directory.GetParent(result.FocExePath).FullName, "__Installer")))
                return false;
            return true;
        }


        private static void FixPossibleOriginBug(GameDetection result)
        {
            var exeDir = new DirectoryInfo(result.FocExePath);
            if (exeDir.Name == "corruption")
            {
                var parentPath = exeDir.Parent?.FullName;
                if (parentPath == null)
                    return;

                var correctedPath = Path.Combine(parentPath, "EAWX");
                if (Directory.Exists(correctedPath))
                    result.FocExePath = correctedPath;
            }
        }
    }
}