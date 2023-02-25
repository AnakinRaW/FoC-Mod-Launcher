using System;
using AnakinRaW.AppUpaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpaterFramework.Updater.Backup;

internal interface IBackupManager
{
    void BackupComponent(IInstallableComponent component);

    void RestoreBackup(IInstallableComponent component);

    void RestoreAll();

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

    public void RestoreBackup(IInstallableComponent component)
    {
        
    }

    public void RestoreAll()
    {
    }

    public void RemoveBackups()
    {
        
    }
}