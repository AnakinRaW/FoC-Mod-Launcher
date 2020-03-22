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
        public static void RestartAndExecutePendingComponents(IRestartOptions restartOptions, IReadOnlyList<IComponent> pendingComponents)
        {
            if (!UpdateConfiguration.Instance.SupportsRestart)
                throw new RestartDeniedOrFailedException("Application restart is not supported.");

            if (restartOptions is null)
                throw new ArgumentNullException(nameof(restartOptions));

            RestartApplication(restartOptions, Elevator.IsProcessElevated);

            //if (!pendingComponents.Any())
            //{
            //    RestartApplication(restartOptions, Elevator.IsProcessElevated);
            //}
            //else
            //{
            //    var updaterTool = UpdateConfiguration.Instance.ExternalUpdaterPath;
            //    if (string.IsNullOrEmpty(updaterTool) || !File.Exists(updaterTool))
            //        throw new RestartDeniedOrFailedException("External updater tool not found");
            //}
        }

        public static void RestartApplication(IRestartOptions restartOptions, bool elevated)
        {
            if (!UpdateConfiguration.Instance.SupportsRestart)
                throw new RestartDeniedOrFailedException("Application restart is not supported.");

            if (restartOptions is null)
                throw new ArgumentNullException(nameof(restartOptions));

            var updaterTool = UpdateConfiguration.Instance.ExternalUpdaterPath;
            if (string.IsNullOrEmpty(updaterTool) || !File.Exists(updaterTool))
                throw new RestartDeniedOrFailedException("External updater tool not found");

            var startInfo = new ProcessStartInfo(updaterTool)
            {
                //CreateNoWindow = true, 
                //WindowStyle = ProcessWindowStyle.Hidden,
            };

            if (elevated) 
                startInfo.Verb = "runas";

            startInfo.Arguments = restartOptions.Unparse();
            Process.Start(startInfo);

            Environment.Exit(0);
        }

        private static void RestartAndExecuteCore(string updater, IEnumerable<IComponent> pendingComponents)
        {

        }
    }
}
