using System;
using System.Diagnostics;
using System.IO;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.Elevation;

namespace TaskBasedUpdater.Restart
{
    public static class ApplicationRestartManager
    {
        public static void RestartApplication(IRestartOptions restartOptions)
        {
            RestartApplication(restartOptions, Elevator.IsProcessElevated);
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
#if !DEBUG
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
#endif
            };

            if (elevated) 
                startInfo.Verb = "runas";

            startInfo.Arguments = restartOptions.Unparse();
            Process.Start(startInfo);

            Environment.Exit(0);
        }
    }
}
