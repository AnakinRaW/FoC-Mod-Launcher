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

        private UpdateConfiguration()
        {
        }
    }
}
