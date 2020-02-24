namespace FocLauncherHost.Updater
{
    public class UpdateConfiguration
    {
        private static UpdateConfiguration _instance;

        public static UpdateConfiguration Instance => _instance ??= new UpdateConfiguration();

        public string? BackupPath { get; set; }

        public BackupPolicy BackupPolicy { get; set; }

        public int ConcurrentDownloads { get; set; } = 2;

        public bool DiagnosticMode { get; set; }

        public int DownloadRetryDelay { get; set; } = 5000;

        public bool AllowEmptyFileDownload { get; set; }

        public ValidationPolicy ValidationPolicy { get; set; }

        public bool DownloadOnlyMode { get; set; }

        public int DownloadRetryCount { get; set; } = 3;

        private UpdateConfiguration()
        {
        }
    }

    public enum ValidationPolicy
    {
        AllowSkipWhenContextNullOrBroken,
        Enforce,
    }
}
