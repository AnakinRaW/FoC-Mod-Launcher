using System;
using System.Collections.Generic;
using System.Linq;
using FocLauncher.Shared;

namespace FocLauncher.AppUpdater
{
    internal class ExternalUpdater
    {
        // Layout - Key: File, Value: Backup
        private readonly Dictionary<string, string> _backups;

        private IReadOnlyCollection<LauncherUpdaterItem> UpdaterItems { get; }
        
        public ExternalUpdater(IReadOnlyCollection<LauncherUpdaterItem> updaterItems)
        {
            UpdaterItems = updaterItems;

            _backups = updaterItems.Where(x => x.Backup != null).ToDictionary(x => x.File, x => x.Backup);

            foreach (var pair in _backups) 
                Console.WriteLine($"Backup added: {pair}");
        }

        public ExternalUpdaterResult Run()
        {
            try
            {
                foreach (var item in UpdaterItems)
                {
                    Console.WriteLine($"Processing item: {item}");
                    FileUtilities.MoveFile(item.File, item.Destination, true);
                    Console.ReadKey();
                }
                return ExternalUpdaterResult.UpdateSuccess;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while updating: {e.Message}");
                Console.WriteLine("Restore backup");

                try
                {
                    foreach (var backup in _backups)
                        FileUtilities.MoveFile(backup.Value, backup.Key, true);
                    return ExternalUpdaterResult.UpdateFailedWithRestore;
                }
                catch (Exception backupException)
                {
                    Console.WriteLine($"Error while restoring backup: {backupException.Message}");
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
                Console.WriteLine($"Cleaning failed with error: {e.Message}");
            }
        }
    }
}