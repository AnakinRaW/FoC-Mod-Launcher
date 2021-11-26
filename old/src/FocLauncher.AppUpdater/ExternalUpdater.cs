using System;
using System.Collections.Generic;
using System.Linq;
using FocLauncher.Shared;
using Newtonsoft.Json;
using NLog;

namespace FocLauncher.AppUpdater
{
    internal class ExternalUpdater
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Layout - Key: BackupDestination, Value: Backup
        private readonly Dictionary<string, string> _backups;

        private IReadOnlyCollection<LauncherUpdaterItem> UpdaterItems { get; }
        
        public ExternalUpdater(IReadOnlyCollection<LauncherUpdaterItem> updaterItems)
        {
            Logger.Debug(JsonConvert.SerializeObject(updaterItems, Formatting.Indented));
            UpdaterItems = updaterItems;
            _backups = updaterItems.Where(x => x.BackupDestination != null).ToDictionary(x => x.BackupDestination, x => x.Backup);
            foreach (var pair in _backups)
                Logger.Debug($"Backup added: {pair}");
        }

        public ExternalUpdaterResult Run()
        {
            try
            {
                foreach (var item in UpdaterItems)
                {
                    Logger.Debug($"Processing item: {item}");
                    if (string.IsNullOrEmpty(item.File))
                        continue;
                    FileUtilities.MoveFile(item.File, item.Destination, true);
                }
                return ExternalUpdaterResult.UpdateSuccess;
            }
            catch (Exception e)
            {
                Logger.Error($"Error while updating: {e.Message}");
                Logger.Debug("Restore backup");

                try
                {
                    foreach (var backup in _backups)
                    {
                        Logger.Debug($"Restore item: {backup}");
                        if (string.IsNullOrEmpty(backup.Value))
                            FileUtilities.DeleteFileWithRetry(backup.Key);
                        else
                            FileUtilities.MoveFile(backup.Value, backup.Key, true);
                    }
                    return ExternalUpdaterResult.UpdateFailedWithRestore;
                }
                catch (Exception backupException)
                {
                    Logger.Error($"Error while restoring backup: {backupException.Message}");
                    return ExternalUpdaterResult.UpdateFailedNoRestore;
                }
            }
            finally
            {
                Clean();
            }
        }

        private void Clean()
        {
            try
            {
                foreach (var item in UpdaterItems)
                {
                    if (!string.IsNullOrEmpty(item.File))
                        FileUtilities.DeleteFileWithRetry(item.File);
                    if (!string.IsNullOrEmpty(item.Backup))
                        FileUtilities.DeleteFileWithRetry(item.Backup);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Cleaning failed with error: {e.Message}");
            }
        }
    }
}