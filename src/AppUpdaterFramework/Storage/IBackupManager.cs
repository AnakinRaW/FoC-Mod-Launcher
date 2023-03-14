using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Storage;

internal interface IBackupManager
{
    void BackupComponent(IInstallableComponent component);

    void RestoreBackup(IInstallableComponent component);

    void RestoreAll();

    void RemoveBackups();
}