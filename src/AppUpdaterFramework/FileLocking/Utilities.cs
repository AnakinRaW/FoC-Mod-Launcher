using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vanara.PInvoke;

namespace AnakinRaW.AppUpdaterFramework.FileLocking;

internal static class Utilities
{
    private static readonly Process CurrentProcess = Process.GetCurrentProcess();

    public static bool ContainsCurrentProcess(this IEnumerable<ILockingProcessInfo> processes)
    {
        return processes.Any(x => x.IsCurrentProcess());
    }

    public static IEnumerable<ILockingProcessInfo> WithoutCurrentProcess(this IEnumerable<ILockingProcessInfo> processes)
    {
        return processes.Where(x => !x.IsCurrentProcess()).ToList();
    }

    public static IEnumerable<ILockingProcessInfo> WithoutStopped(this IEnumerable<ILockingProcessInfo> processes)
    {
        return processes.Where(x => !x.IsStopped()).ToList();
    }

    public static bool AllStopped(this IEnumerable<ILockingProcessInfo> processes)
    { 
        return processes.All(x => x.IsStopped());
    }

    public static bool AnyRunning(this IEnumerable<ILockingProcessInfo> processes)
    {
        return processes.Any(x => x.ApplicationStatus is RstrtMgr.RM_APP_STATUS.RmStatusUnknown or RstrtMgr.RM_APP_STATUS.RmStatusRunning or RstrtMgr.RM_APP_STATUS.RmStatusRestarted);
    }

    public static bool IsCurrentProcess(this ILockingProcessInfo process)
    {
        return process.Id == CurrentProcess.Id;
    }

    public static bool IsStopped(this ILockingProcessInfo process)
    {
        return process.ApplicationStatus is RstrtMgr.RM_APP_STATUS.RmStatusStopped
            or RstrtMgr.RM_APP_STATUS.RmStatusStoppedOther;
    }
}