using System;

namespace AnakinRaW.AppUpdaterFramework.FileLocking;

internal interface ILockingProcessInfo
{
    uint Id { get; }

    DateTime StartTime { get; }

    string Description { get; }

    string ServiceName { get; }

    Vanara.PInvoke.RstrtMgr.RM_APP_TYPE ApplicationType { get; }

    Vanara.PInvoke.RstrtMgr.RM_APP_STATUS ApplicationStatus { get; }

    bool IsRestartable { get; }
}