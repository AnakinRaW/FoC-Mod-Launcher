using System;
using System.IO;
using System.Linq;
using FocLauncher.Game.Detection;

namespace FocLauncher.Game
{
    public static class GameTypeHelper
    {
        // TODO: Test all checks again 
        internal static GameType GetGameType(GameDetection result)
        {
            if (result.IsError)
                return GameType.Undefined;
            if (CheckSteam(result.FocExe))
                return GameType.SteamGold;
            if (CheckGoG(result.FocExe))
                return GameType.GoG;
            // TODO: This should not mutate the object anymore. It then can probably be made to an struct again
            if (CheckOrigin(result))
                return GameType.Origin;
            // TODO: Check DiskGold (just to have them all)
            return GameType.Disk;
        }
        
        private static bool CheckSteam(FileInfo exeFile)
        {
            var directory = exeFile.Directory;
            if (directory == null || !directory.Name.Equals("corruption", StringComparison.InvariantCultureIgnoreCase))
                return false;

            var gameData = directory.Parent?.GetDirectories().FirstOrDefault(x => x.Name.Equals("GameData"));
            if (gameData == null)
                return false;

            if (!gameData.GetFiles().Any(x => x.Name.Equals("sweaw.exe")))
                return false;

            var parentFiles = directory.Parent.GetFiles();
            return parentFiles.Any(x => x.Name.Equals("runm2.dat")) && parentFiles.Any(x => x.Name.Equals("runme.dat"));
        }

        private static bool CheckGoG(FileInfo path)
        {
            //if (new DirectoryInfo(path).Name != "EAWX")
            //    return false;
            //if (!File.Exists(Directory.GetParent(path) + "\\GameData\\sweaw.exe"))
            //    return false;
            //if (!File.Exists(Directory.GetParent(path) + "\\GameData\\goggame-1421404887.dll"))
            //    return false;
            return true;
        }

        private static bool CheckOrigin(GameDetection result)
        {
            FixPossibleOriginBug(result);
            //if (new DirectoryInfo(result.FocExe).Name != "EAWX")
            //    return false;
            //if (!Directory.Exists(Path.Combine(Directory.GetParent(result.FocExe).FullName, "Manuals")))
            //    return false;
            //if (!Directory.Exists(Path.Combine(Directory.GetParent(result.FocExe).FullName, "__Installer")))
            //    return false;
            return true;
        }


        private static void FixPossibleOriginBug(GameDetection result)
        {
            //var exeDir = new DirectoryInfo(result.FocExe);
            //if (exeDir.Name == "corruption")
            //{
            //    var parentPath = exeDir.Parent?.FullName;
            //    if (parentPath == null)
            //        return;

            //    var correctedPath = Path.Combine(parentPath, "EAWX");
            //    if (Directory.Exists(correctedPath))
            //        result.FocExe = correctedPath;
            //}
        }
    }
}