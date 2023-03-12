namespace AnakinRaW.AppUpdaterFramework.Installer;

internal enum InstallOperationResult
{
    Success,
    Failed,
    Canceled,
    NoPermission,
    LockedFile,
}