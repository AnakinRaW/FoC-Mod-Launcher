using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TaskBasedUpdater.NativeMethods;

namespace TaskBasedUpdater.Restart
{
    public static class LockingProcessManagerFactory
    {
        public static ILockingProcessManager Create()
        {
            var strSessionKey = new StringBuilder(32);
            LockingProcessManager.ThrowOnError(RestartMgr.RmStartSession(out var pSessionHandle, 0, strSessionKey));
            return new LockingProcessManager(pSessionHandle, strSessionKey.ToString());
        }
    }

    public static class LockingProcessManagerUtilities
    {
        public static bool ProcessesContainsSelf(this ILockingProcessManager processManager)
        {
            if (processManager is null)
                throw new ArgumentNullException(nameof(processManager));
            var currentProcess = Process.GetCurrentProcess();
            return processManager.GetProcesses().Any(x => x.Id.Equals(currentProcess.Id));
        }

        public static bool ProcessesContainsOnlySelf(this ILockingProcessManager processManager)
        {
            if (processManager is null)
                throw new ArgumentNullException(nameof(processManager));
            return processManager.GetProcesses().Count() <= 1 && processManager.ProcessesContainsSelf();
        }
    }
}