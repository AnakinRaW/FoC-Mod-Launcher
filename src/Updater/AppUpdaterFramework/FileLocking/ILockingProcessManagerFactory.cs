namespace AnakinRaW.AppUpdaterFramework.FileLocking;

internal interface ILockingProcessManagerFactory
{
    ILockingProcessManager Create();
}