namespace AnakinRaW.AppUpdaterFramework.FileLocking;

public interface ILockingProcessInfo
{
    int Id { get; }

    Vanara.PInvoke.RstrtMgr.RM_APP_TYPE ApplicationType { get; }

    Vanara.PInvoke.RstrtMgr.RM_APP_STATUS ApplicationStatus { get; }
}