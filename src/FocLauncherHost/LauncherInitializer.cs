﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using FocLauncher;
using FocLauncher.Shared;
using FocLauncherHost.Dialogs;
using FocLauncherHost.Update.UpdateCatalog;
using FocLauncherHost.Utilities;
using NLog;

namespace FocLauncherHost
{
    internal static class LauncherInitializer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static bool Initialize(ExternalUpdaterResult launchOption)
        {
            if (!Directory.Exists(LauncherConstants.ApplicationBasePath))
                throw new DirectoryNotFoundException($"Required directory '{LauncherConstants.ApplicationBasePath}' not found.");

            try
            {
                if (CheckRestoreRequired(launchOption)) 
                    Restore();
                HandleLastUpdateResult(launchOption, out var skipWriteToDisk);
                if (!skipWriteToDisk)
                {
                    AssemblyExtractor.WriteNecessaryAssembliesToDisk(LauncherConstants.ApplicationBasePath,
                        "FocLauncher.dll",
                        "FocLauncher.Theming.dll",
                        LauncherConstants.UpdaterFileName,
                        "FocLauncher.Threading.dll",
                        "Microsoft.VisualStudio.Utilities.dll");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                if (e is AggregateException aggregate)
                    e = aggregate.GetBaseException();
                LauncherRegistryHelper.WriteValue(LauncherRegistryKeys.ForceRestore, true);
                // TODO: This and the exception dialog should be the same dialog
                new RestartSystemDialog(e.Message).ShowDialog();
                Environment.Exit(0);
            }

            LogInstalledAssemblies();
            SetupRegistry();
            SetCurrentUpdateSearchMode();
            return launchOption == ExternalUpdaterResult.NoUpdate;
        }

        private static void SetCurrentUpdateSearchMode()
        {
            if ((Keyboard.GetKeyStates(Key.T) & KeyStates.Down) > 0)
            {
                FocLauncherInformation.Instance.CurrentUpdateSearchOption = ApplicationType.Test;
                return;
            }
            if ((Keyboard.GetKeyStates(Key.B) & KeyStates.Down) > 0)
            {
                FocLauncherInformation.Instance.CurrentUpdateSearchOption = ApplicationType.Beta;
                return;
            }
            FocLauncherInformation.Instance.CurrentUpdateSearchOption = null;
        }

        private static void SetupRegistry()
        {
            LauncherRegistryHelper.Initialize();
        }

        private static void HandleLastUpdateResult(ExternalUpdaterResult launchOption, out bool skipWrite)
        {
            skipWrite = false;
            if (launchOption == ExternalUpdaterResult.UpdateFailedWithRestore)
            {
                new UpdateResultDialog("Update failed",
                    "The update of the launcher failed but it recovered itself to the last working state.").ShowDialog();
                skipWrite = true;
            }

            if (launchOption == ExternalUpdaterResult.UpdateFailedNoRestore)
            {
                new RestoreDialog(false).ShowDialog();
            }

            if (launchOption == ExternalUpdaterResult.UpdateSuccess)
            {
                new UpdateSuccessDialog().ShowDialog();
                skipWrite = true;
            }
        }


        private static void Restore()
        {
           Logger.Debug("Performing full restore by deleting the application's base directory.");
           Directory.Delete(LauncherConstants.ApplicationBasePath, true);
           Directory.CreateDirectory(LauncherConstants.ApplicationBasePath);
           LauncherRegistryHelper.DeleteValue(LauncherRegistryKeys.ForceRestore);
        }

        private static bool CheckRestoreRequired(ExternalUpdaterResult launchOption)
        {
            Logger.Debug("Check if a full restore is required.");
            var restore = false;
            if (launchOption == ExternalUpdaterResult.UpdateFailedNoRestore)
                restore = true;
            else if (launchOption == ExternalUpdaterResult.DemandsRestore)
                restore = true;
            else if ((Keyboard.Modifiers & ModifierKeys.Shift) > 0)
                restore = true;
            var successRegistry = LauncherRegistryHelper.GetValue<bool>(LauncherRegistryKeys.ForceRestore, out var forceRestore);
            if (!successRegistry || forceRestore)
                restore = true;
            Logger.Debug(restore);
            return restore;
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
    }
}