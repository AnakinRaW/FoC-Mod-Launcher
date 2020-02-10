namespace FocLauncherHost.Updater
{
    public enum InstallResult
    {
        None,
        Success,
        SuccessRestartRequired,
        Failure,
        FailureException,
        Cancel,
    }
}