using System.Collections.Generic;
using System.Linq;

namespace AnakinRaW.AppUpdaterFramework.FileLocking;

internal class LockingProcessManager : ILockingProcessManager
{
    public LockingProcessManager(int sessionId, string sessionKey)
    {
    }

    public void Dispose()
    {
    }

    public void Register(IEnumerable<string> files = null, IEnumerable<ILockingProcessInfo> processes = null)
    {
    }

    public void TerminateRegisteredProcesses()
    {
    }

    public IEnumerable<ILockingProcessInfo> GetProcesses()
    {
        return Enumerable.Empty<ILockingProcessInfo>();
    }
}