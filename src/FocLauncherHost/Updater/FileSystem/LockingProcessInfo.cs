using System;
using FocLauncherHost.Updater.NativeMethods;

namespace FocLauncherHost.Updater.FileSystem
{
    internal class LockingProcessInfo
    {
        public int Id { get; }

        public DateTime StartTime { get; }

        public string Description { get; }

        public string ServiceName { get; }

        public RestartMgr.ApplicationType ApplicationType { get; }

        public RestartMgr.ApplicationStatus ApplicationStatus { get; }

        public bool IsRestartable { get; }

        internal LockingProcessInfo(RestartMgr.RmProcessInfo process)
        {
            var fileTime = ((long)process.Process.ProcessStartTime.dwHighDateTime << 32) + (long)process.Process.ProcessStartTime.dwLowDateTime;
            Id = process.Process.DwProcessId;
            StartTime = DateTime.FromFileTime(fileTime);
            Description = process.strAppName;
            ServiceName = process.strServiceShortName;
            ApplicationType = process.ApplicationType;
            ApplicationStatus = process.AppStatus;
            IsRestartable = process.bRestartable;
        }

    }
}