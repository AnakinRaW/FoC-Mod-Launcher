using System;

namespace FocLauncherHost.Updater.Restart
{
    public interface ILockingProcessInfo
    {
        int Id { get; }

        DateTime StartTime { get; }

        string Description { get; }

        string ServiceName { get; }

        ApplicationType ApplicationType { get; }

        ApplicationStatus ApplicationStatus { get; }

        bool IsRestartable { get; }
    }
}