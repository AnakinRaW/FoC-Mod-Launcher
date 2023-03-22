using System;
using Vanara.PInvoke;

namespace AnakinRaW.AppUpdaterFramework.FileLocking;

internal interface ILockingProcessInfo
{
    uint Id { get; }

    DateTime StartTime { get; }

    public string Description { get; }

    public string ServiceName { get; }

    RstrtMgr.RM_APP_TYPE ApplicationType { get; }

    RstrtMgr.RM_APP_STATUS ApplicationStatus { get; }
}