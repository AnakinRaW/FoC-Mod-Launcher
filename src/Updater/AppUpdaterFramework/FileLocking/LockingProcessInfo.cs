using System;
using Vanara.PInvoke;

namespace AnakinRaW.AppUpdaterFramework.FileLocking;

internal class LockingProcessInfo : ILockingProcessInfo
{
    public uint Id { get; }

    public DateTime StartTime { get; }

    public string Description { get; }

    public string ServiceName { get; }

    public RstrtMgr.RM_APP_TYPE ApplicationType { get; }

    public RstrtMgr.RM_APP_STATUS ApplicationStatus { get; }

    internal LockingProcessInfo(RstrtMgr.RM_PROCESS_INFO process)
    {
        var fileTime = ((long)process.Process.ProcessStartTime.dwHighDateTime << 32) + process.Process.ProcessStartTime.dwLowDateTime;
        Id = process.Process.dwProcessId;
        StartTime = DateTime.FromFileTime(fileTime);
        Description = process.strAppName;
        ServiceName = process.strServiceShortName;
        ApplicationType = process.ApplicationType;
        ApplicationStatus = process.AppStatus;
    }
}