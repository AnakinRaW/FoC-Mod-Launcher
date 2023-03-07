namespace FocLauncher.Update.ViewModels;

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