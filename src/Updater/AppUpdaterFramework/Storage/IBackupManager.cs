using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Storage;

internal interface IBackupManager
{
    IDictionary<IInstallableComponent, BackupValueData> Backups { get; }

    void BackupComponent(IInstallableComponent component);

    void RestoreBackup(IInstallableComponent component);

    void RestoreAll();

    void RemoveBackups();

    void RemoveBackup(IInstallableComponent component);
}