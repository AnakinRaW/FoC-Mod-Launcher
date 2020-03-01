using System;

namespace TaskBasedUpdater.Download
{
    public class DownloadFailureInformation
    {
        public Exception Exception { get; set; }
        public string Engine { get; set; }

        public DownloadFailureInformation()
        {
        }

        public DownloadFailureInformation(Exception exception, string engine)
        {
            Exception = exception;
            Engine = engine;
        }
    }
}