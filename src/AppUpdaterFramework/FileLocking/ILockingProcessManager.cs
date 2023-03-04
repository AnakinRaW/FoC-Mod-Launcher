using System;
using System.Collections.Generic;

namespace AnakinRaW.AppUpdaterFramework.FileLocking;

internal interface ILockingProcessManager : IDisposable
{
    void Register(IEnumerable<string>? files = null, IEnumerable<ILockingProcessInfo>? processes = null);

    void TerminateRegisteredProcesses();

    IEnumerable<ILockingProcessInfo> GetProcesses();
}