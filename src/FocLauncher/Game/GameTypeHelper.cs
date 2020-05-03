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
            if (CheckOrigin(result, out var fixedFileInfo))
            {
                if (fixedFileInfo != null)
                    result.FocExe = fixedFileInfo;

                return GameType.Origin;
            }

            // TODO: Check DiskGold (just to have them all)
            return GameType.Disk;
        }


        public static GameType GetGameType(FileInfo focExe)
        {
            if (CheckSteam(focExe))
                return GameType.SteamGold;
            if (CheckGoG(focExe))
                return GameType.GoG;
            if (CheckOrigin(focExe)) 
                return GameType.Origin;
            // TODO: Check DiskGold (just to have them all)
            return GameType.Disk;
        }



        private static bool CheckSteam(FileInfo exeFile)
        {
            if (exeFile is null || !exeFile.Exists)
                return false;

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

        private static bool CheckGoG(FileInfo exeFile)
        {
            if (exeFile is null || !exeFile.Exists)
                return false;
            if (exeFile.Directory?.Name != "EAWX")
                return false;
            var eawPath = exeFile.Directory.Parent?.EnumerateDirectories().FirstOrDefault(x => x.Name.Equals("GameData"));
            if (eawPath == null)
                return false;
            var fileNames = eawPath.EnumerateFiles().Select(x => x.Name).ToList();
            return fileNames.Any(x => x.Equals("sweaw.exe")) && fileNames.Any(x => x.Equals("goggame-1421404887.dll"));
        }

        private static bool CheckOrigin(GameDetection result, out FileInfo? fixedFileInfo)
        {
            fixedFileInfo = default;

            var focExe = result.FocExe;
            var exists = CheckOrigin(focExe);
            if (exists)
                return true;

            var fixedFocExe = CreateOriginFileInfo(focExe);
            if (fixedFocExe is null)
                return false;

            var fixWorked = CheckOrigin(fixedFileInfo);
            if (fixWorked)
            {
                fixedFileInfo = fixedFocExe;
                return true;
            }

            return false;
        }

        private static bool CheckOrigin(FileInfo exeFile)
        {
            var directory = exeFile?.Directory;
            if (directory is null)
                return false;

            if ( exeFile.Exists || !directory.Exists)
                return false;

            if (!directory.EnumerateFiles().Any(x => x.Name.Equals("EALaunchHelper.exe")))
                return false;

            var parent = directory.Parent;
            if (parent is null)
                return false;

            if (!parent.EnumerateDirectories().Any(x => x.Name.Equals("Manuals")) ||
                !parent.EnumerateDirectories().Any(x => x.Name.Equals("__Installer")))
                return false;

            return true;
        }


        private static FileInfo? CreateOriginFileInfo(FileInfo focExeFile)
        {
            if (focExeFile is null)
                throw new ArgumentNullException(nameof(focExeFile));

            var exeDir = focExeFile.Directory;
            if (exeDir is null)
                return null;

            if (exeDir.Name.Equals("EAWX"))
                return focExeFile;

            if (exeDir.Name.Equals("corruption"))
            {
                var parentPath = exeDir.Parent?.FullName;
                if (parentPath == null)
                    return null;
                var correctedPath = Path.Combine(parentPath, "EAWX", focExeFile.Name);
                return new FileInfo(correctedPath);
            }

            return null;
        }
    }
}