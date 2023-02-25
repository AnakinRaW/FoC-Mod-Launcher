using System;
using AnakinRaW.AppUpaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpaterFramework.Updater.Backup;

internal interface IBackupManager
{
    void BackupComponent(IInstallableComponent component);

    void RemoveBackups();
}

internal class BackupManager : IBackupManager
{
    public BackupManager(IServiceProvider serviceProvider)
    {
        
    }

    public void BackupComponent(IInstallableComponent component)
    {
    }

    public void RemoveBackups()
    {
        
    }
}