using System;
using System.Collections.Generic;
using System.Linq;
using FocLauncher.Shared;
using NLog;

namespace FocLauncher.AppUpdater
{
    internal class ExternalUpdater
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Layout - Key: File, Value: Backup
        private readonly Dictionary<string, string> _backups;

        private IReadOnlyCollection<LauncherUpdaterItem> UpdaterItems { get; }
        
        public ExternalUpdater(IReadOnlyCollection<LauncherUpdaterItem> updaterItems)
        {
            UpdaterItems = updaterItems;
            _backups = updaterItems.Where(x => x.Backup != null).ToDictionary(x => x.File, x => x.Backup);
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
                        FileUtilities.MoveFile(backup.Value, backup.Key, true);
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