using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FocLauncherHost.Updater.Component;
using FocLauncherHost.Updater.Configuration;

namespace FocLauncherHost.Updater.Restart
{
    internal static class ApplicationRestartManager
    {
        public static void RestartAndExecutePendingComponents(IEnumerable<IComponent> pendingComponents)
        {
            if (!UpdateConfiguration.Instance.SupportsRestart)
                throw new RestartDeniedOrFailedException("Application restart is not supported.");
            
            if (!pendingComponents.Any())
            {
                RestartApplication(Elevator.IsProcessElevated);
            }
            var updaterTool = UpdateConfiguration.Instance.ExternalUpdaterPath;
            if (string.IsNullOrEmpty(updaterTool) || !File.Exists(updaterTool))
                throw new RestartDeniedOrFailedException("External updater tool not found");
        }

        public static void RestartApplication(bool elevated)
        {
            throw new NotImplementedException();
        }

        private static void RestartAndExecuteCore(string updater, IEnumerable<IComponent> pendingComponents)
        {

        }
    }
}
