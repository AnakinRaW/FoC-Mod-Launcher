using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.Elevation;

namespace TaskBasedUpdater.Restart
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
            else
            {
                var updaterTool = UpdateConfiguration.Instance.ExternalUpdaterPath;
                if (string.IsNullOrEmpty(updaterTool) || !File.Exists(updaterTool))
                    throw new RestartDeniedOrFailedException("External updater tool not found");
            }
        }

        public static void RestartApplication(bool elevated)
        {
            if (!UpdateConfiguration.Instance.SupportsRestart)
                throw new RestartDeniedOrFailedException("Application restart is not supported.");

            var updaterTool = UpdateConfiguration.Instance.ExternalUpdaterPath;
            if (string.IsNullOrEmpty(updaterTool) || !File.Exists(updaterTool))
                throw new RestartDeniedOrFailedException("External updater tool not found");

            if (!elevated)
            {

            }


            var elevator = UpdateConfiguration.Instance.ExternalElevatorPath;
            if (!File.Exists(elevator))
                throw new RestartDeniedOrFailedException("Elevator tool not found");

            var startInfo = new ProcessStartInfo(elevator) {CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden};

            var currentProcessInfo = ProcessUtilities.GetCurrentProcessInfo();

            startInfo.Arguments = $"{currentProcessInfo.Id} {currentProcessInfo.Arguments}";
            Process.Start(startInfo);

            Environment.Exit(0);
        }

        private static void RestartAndExecuteCore(string updater, IEnumerable<IComponent> pendingComponents)
        {

        }
    }
}
