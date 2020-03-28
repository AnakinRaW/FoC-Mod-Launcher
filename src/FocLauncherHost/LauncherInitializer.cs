using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using FocLauncher;
using FocLauncher.Shared;
using FocLauncherHost.Dialogs;
using FocLauncherHost.Utilities;
using NLog;

namespace FocLauncherHost
{
    internal class LauncherInitializer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static bool Initialize(ExternalUpdaterResult launchOption)
        {
            if (!Directory.Exists(LauncherConstants.ApplicationBasePath))
                throw new DirectoryNotFoundException($"Required directory '{LauncherConstants.ApplicationBasePath}' not found.");

            try
            {
                RestoreIfNecessary(launchOption);

                var skipWriteToDisk = false;
                if (launchOption == ExternalUpdaterResult.UpdateFailedWithRestore)
                {
                    skipWriteToDisk = true;
                    new UpdateResultDialog("Update failed", 
                        "The update of the launcher failed but it recovered itself to the last working state.").ShowDialog();
                }

                if (launchOption == ExternalUpdaterResult.UpdateFailedNoRestore)
                {
                    new RestoreDialog(false).ShowDialog();
                }

                if (launchOption == ExternalUpdaterResult.UpdateSuccess)
                {
                    skipWriteToDisk = true;
                    new UpdateSuccessDialog().ShowDialog();
                }

                if (!skipWriteToDisk)
                {
                    AssemblyExtractor.WriteNecessaryAssembliesToDisk(LauncherConstants.ApplicationBasePath,
                        "FocLauncher.dll",
                        "FocLauncher.Theming.dll",
                        LauncherConstants.UpdaterFileName,
                        "FocLauncher.Threading.dll",
                        "Microsoft.VisualStudio.Utilities.dll");
                }
                

                LogInstalledAssemblies();

            }
            catch (Exception e)
            {
                //Logger.Error(e);
                if (e is AggregateException aggregate)
                    e = aggregate.GetBaseException();

                // TODO: This and the exception dialog should be the same dialog
                new RestartSystemDialog(e.Message).ShowDialog();
                
                // TODO: Set restore for next start in registry

                Environment.Exit(0);
            }

            return launchOption != ExternalUpdaterResult.NoUpdate;
        }

        private static void LogInstalledAssemblies()
        {
            var fileList = new List<string> { Environment.GetCommandLineArgs()[0] };
            var appBase = new DirectoryInfo(LauncherConstants.ApplicationBasePath);
            fileList.AddRange(appBase.GetFilesByExtensions(false, ".exe", ".dll").Select(x => x.FullName));

            Logger.Debug("****Installed Files*****");
            foreach (var file in fileList)
            {
                var fileVersion = FileVersionInfo.GetVersionInfo(file).FileVersion;
                var fileName = Path.GetFileName(file);
                Logger.Debug($"\tFile: {fileName}, File-Version: {fileVersion}");
            }
        }

        private static void RestoreIfNecessary(ExternalUpdaterResult launchOption)
        {
            Logger.Debug("Check if a full restore is required.");
            var restore = false;
            if (launchOption == ExternalUpdaterResult.UpdateFailedNoRestore)
                restore = true;
            else if (launchOption == ExternalUpdaterResult.DemandsRestore)
                restore = true;
            else if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0)
                restore = true;
            // TODO Registry check
            Logger.Debug(restore);
            if (!restore)
                return;

            Logger.Debug("Performing full restore by deleting the application's base directory.");
            Directory.Delete(LauncherConstants.ApplicationBasePath, true);

            // TODO: Reset registry 
        }
    }
}