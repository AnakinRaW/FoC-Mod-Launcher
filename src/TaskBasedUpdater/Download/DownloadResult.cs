namespace TaskBasedUpdater.Download
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