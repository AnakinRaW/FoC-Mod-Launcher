namespace AnakinRaW.AppUpdaterFramework.FileLocking.Interaction;

public interface ILockingProcess
{
    string ProcessName { get; }

    uint ProcessId { get; }

    internal record LockingProcess(string ProcessName, uint ProcessId) : ILockingProcess;
}