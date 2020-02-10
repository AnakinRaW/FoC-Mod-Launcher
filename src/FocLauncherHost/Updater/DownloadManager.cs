namespace FocLauncherHost.Updater
{
    internal class DownloadManager
    {
        private static DownloadManager _instance;

        public static DownloadManager Instance => _instance ??= new DownloadManager();
    }
}