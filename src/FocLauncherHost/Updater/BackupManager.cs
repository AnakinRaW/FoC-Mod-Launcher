using FocLauncherHost.Updater.Component;

namespace FocLauncherHost.Updater
{
    internal class BackupManager
    {
        private static BackupManager _instance;
        public static BackupManager Instance => _instance ??= new BackupManager();

        private BackupManager()
        {

        }

        public void CreateBackup(IComponent component)
        {

        }

        public void RemoveAllBackups()
        {

        }
    }
}
