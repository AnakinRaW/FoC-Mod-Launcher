using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace FocLauncher.Game
{
    public static class GameStartHelper
    {
        public static void StartGameProcess(Process process, string iconPath)
        {
            if (process == null)
                return;

            var fileName = process.StartInfo.FileName;
            var a = process.StartInfo.Arguments;

            var linkPath = Path.Combine(LauncherDataModel.AppDataPath, "tmp.lnk");

            CreateShortcut(fileName, linkPath, a, process.StartInfo.WorkingDirectory, iconPath);

            var startingProcess = new Process
            {
                StartInfo = { FileName = linkPath }
            };
            startingProcess.Start();
            Thread.Sleep(2000);
        }

        private static void CreateShortcut(string filePath, string linkPath, string arguments, string wd, string iconPath)
        {
            var link = (NativeMethods.NativeMethods.IShellLink)new NativeMethods.NativeMethods.ShellLink();
            link.SetPath(filePath);
            link.SetWorkingDirectory(wd);
            link.SetArguments(arguments);

            if (iconPath == null || !File.Exists(iconPath))
                iconPath = LauncherDataModel.IconPath;

            link.SetIconLocation(iconPath, 0);
            var file = (IPersistFile)link;
            file.Save(linkPath, false);
        }
    }
}
