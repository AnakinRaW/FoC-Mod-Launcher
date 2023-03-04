using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AnakinRaW.AppUpdaterFramework.FileLocking;

internal static class Utilities
{
    private static readonly Process CurrentProcess = Process.GetCurrentProcess();

    public static bool ContainsCurrentProcess(this IEnumerable<ILockingProcessInfo> processes)
    {
        return processes.Any(x => x.IsCurrentProcess());
    }

    public static bool IsCurrentProcess(this ILockingProcessInfo process)
    {
        return process.Id == CurrentProcess.Id;
    }
}