using System;

namespace TaskBasedUpdater.Restart
{
    [Flags]
    public enum WindowsRestartManagerShutdown
    {
        ForceShutdown = 1,
        ShutdownOnlyRegistered = 16,
    }
}