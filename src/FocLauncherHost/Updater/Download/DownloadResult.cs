namespace FocLauncherHost.Updater.Download
{
    public enum DownloadResult
    {
        Success,
        NotSupported,
        Exception,
        MissingOrInvalidValidationContext,
        HashMismatch,
    }
}