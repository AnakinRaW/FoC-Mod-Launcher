namespace AnakinRaW.AppUpaterFramework.Updater.Tasks;

public enum InstallResult
{
    None,
    Success,
    SuccessRestartRequired,
    Failure,
    FailureException,
    Cancel,
}