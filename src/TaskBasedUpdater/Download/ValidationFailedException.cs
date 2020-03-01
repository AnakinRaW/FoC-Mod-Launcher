namespace TaskBasedUpdater.Download
{
    public class ValidationFailedException : UpdaterException
    {
        public DownloadResult Result { get; }

        public ValidationFailedException(DownloadResult result, string message) : base(message)
        {
            Result = result;
            HResult = -2146869244;
        }
    }
}