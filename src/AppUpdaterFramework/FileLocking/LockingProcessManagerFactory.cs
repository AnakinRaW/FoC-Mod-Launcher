using System.ComponentModel;
using System.Text;

namespace AnakinRaW.AppUpdaterFramework.FileLocking;

internal class LockingProcessManagerFactory : ILockingProcessManagerFactory
{
    public ILockingProcessManager Create()
    {
        var strSessionKey = new StringBuilder(32);
        var result = Vanara.PInvoke.RstrtMgr.RmStartSession(out var pSessionHandle, 0, strSessionKey);
        if (result != 0)
            throw new Win32Exception(result.ToHRESULT().Code);
        return new LockingProcessManager(pSessionHandle);
    }
}