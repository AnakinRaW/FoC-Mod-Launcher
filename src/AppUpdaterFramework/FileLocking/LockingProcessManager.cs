using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Validation;
using Vanara.PInvoke;
namespace AnakinRaW.AppUpdaterFramework.FileLocking;

internal class LockingProcessManager : ILockingProcessManager
{
    private readonly uint _sessionId;
    private readonly string _sessionKey;
    private bool _isDisposed;
    private bool _registered;

    public LockingProcessManager(uint sessionId, string sessionKey)
    {
        _sessionId = sessionId;
        _sessionKey = sessionKey;
    }

    ~LockingProcessManager()
    {
        DisposeCore();
    }


    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    public void Register(IEnumerable<string>? files = null, IEnumerable<ILockingProcessInfo>? processes = null)
    {
        var fileNames = files?.ToArray();
        var fileCount = (uint?) fileNames?.Length ?? 0;
        var processArray = processes?.ToArray();
        var processCount = (uint?) processArray?.Length ?? 0;
        if (fileCount == 0 && processCount == 0)
            return;
        _registered = true;
        var rgProcesses = processArray?.Select(Convert).ToArray();

        var result = RstrtMgr.RmRegisterResources(_sessionId, fileCount, fileNames, processCount, rgProcesses, 0, null);
        if (result.Failed)
            throw new Win32Exception(result.ToHRESULT().Code);
    }

    public void TerminateRegisteredProcesses()
    {
        if (!_registered)
            return;
        var result = RstrtMgr.RmShutdown(_sessionId, RstrtMgr.RM_SHUTDOWN_TYPE.RmForceShutdown);
        if (result.Failed)
            throw new Win32Exception(result.ToHRESULT().Code);
    }

    public IEnumerable<ILockingProcessInfo> GetProcesses()
    {
        if (!_registered) 
            return Enumerable.Empty<ILockingProcessInfo>();
        int result;
        var pnProcInfo = 0U;
        var rmProcessInfoArray = (RstrtMgr.RM_PROCESS_INFO[]?)null;
        do
        {
            result = RstrtMgr.RmGetList(_sessionId, out var pnProcInfoNeeded, ref pnProcInfo, rmProcessInfoArray, out _).ToHRESULT().Code;

            switch (result)
            {
                case 0:
                    break;
                case 234:
                    pnProcInfo = pnProcInfoNeeded;
                    rmProcessInfoArray = new RstrtMgr.RM_PROCESS_INFO[pnProcInfo];
                    break;
                default:
                    throw new Win32Exception(result);
            }
        } while (result == 234);

        if (rmProcessInfoArray != null && rmProcessInfoArray.Length != 0)
            return rmProcessInfoArray.Select(process => new LockingProcessInfo(process)).ToArray();
        return Enumerable.Empty<ILockingProcessInfo>();
    }

    private void DisposeCore()
    {
        if (_isDisposed)
            return;
        var result = RstrtMgr.RmEndSession(_sessionId);
        _isDisposed = true;
        if (result.Failed)
            throw new Win32Exception(result.ToHRESULT().Code);
    }

    private static RstrtMgr.RM_UNIQUE_PROCESS Convert(ILockingProcessInfo process)
    {
        Requires.NotNull(process, nameof(process));
        var fileTimeUtc = process.StartTime.ToFileTimeUtc();
        var fileTime = new FILETIME
        {
            dwHighDateTime = (int)(fileTimeUtc >> 32),
            dwLowDateTime = (int)(fileTimeUtc & uint.MaxValue)
        };
        return new RstrtMgr.RM_UNIQUE_PROCESS
        {
            dwProcessId = (uint) process.Id,
            ProcessStartTime = fileTime
        };
    }
}

internal class LockingProcessInfo : ILockingProcessInfo
{
    public uint Id { get; }

    public DateTime StartTime { get; }

    public string Description { get; }

    public string ServiceName { get; }
    
    public RstrtMgr.RM_APP_TYPE ApplicationType { get; }
    public RstrtMgr.RM_APP_STATUS ApplicationStatus { get; }

    public bool IsRestartable { get; }

    internal LockingProcessInfo(RstrtMgr.RM_PROCESS_INFO process)
    {
        var fileTime = ((long)process.Process.ProcessStartTime.dwHighDateTime << 32) + process.Process.ProcessStartTime.dwLowDateTime;
        Id = process.Process.dwProcessId;
        StartTime = DateTime.FromFileTime(fileTime);
        Description = process.strAppName;
        ServiceName = process.strServiceShortName;
        ApplicationType = process.ApplicationType;
        ApplicationStatus = process.AppStatus;
        IsRestartable = process.bRestartable;
    }

}