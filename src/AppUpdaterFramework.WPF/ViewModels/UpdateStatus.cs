namespace AnakinRaW.AppUpdaterFramework.ViewModels;

public enum UpdateStatus
{
    None,
    CheckingForUpdate,
    UpdateAvailable,
    NoUpdate,
    Updating,
    FailedChecking,
    Cancelled
}