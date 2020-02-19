using System;

namespace FocLauncherHost.Updater.Restart
{
    [Flags]
    public enum WindowsRestartManagerShutdown
    {
        ForceShutdown = 1,
        ShutdownOnlyRegistered = 16,
    }
}