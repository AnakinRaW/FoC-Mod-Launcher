using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.ExternalUpdater.CLI;
using FocLauncher.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AnakinRaW.ExternalUpdater;

internal class ExternalUpdater
{
    private readonly ILogger? _logger;
    private readonly IFileSystemService _fileUtilities;

    private readonly IFileSystem _fileSystem;

    // Layout - Key: BackupDestination, Value: Backup
    private readonly Dictionary<string, string?> _backups;

    private IReadOnlyCollection<LauncherUpdateItem> UpdaterItems { get; }

    public ExternalUpdater(IReadOnlyCollection<LauncherUpdateItem> updaterItems, IServiceProvider serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _fileUtilities = serviceProvider.GetRequiredService<IFileSystemService>();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(Program));
        _logger?.LogDebug(JsonConvert.SerializeObject(updaterItems, Formatting.Indented));
        UpdaterItems = updaterItems;
        _backups = updaterItems.Where(x => x.BackupDestination != null).ToDictionary(x => x.BackupDestination!, x => x.Backup);
        foreach (var pair in _backups)
            _logger?.LogDebug($"Backup added: {pair}");
    }

    public ExternalUpdaterResult Run()
    {
        try
        {
            foreach (var item in UpdaterItems)
            {
                _logger?.LogDebug($"Processing item: {item}");
                if (string.IsNullOrEmpty(item.File))
                    continue;
                var fileInfo = _fileSystem.FileInfo.New(item.File);
                _fileUtilities.MoveFile(fileInfo, item.Destination!, true);
            }
            return ExternalUpdaterResult.UpdateSuccess;
        }
        catch (Exception e)
        {
            _logger?.LogCritical($"Error while updating: {e.Message}");
            _logger?.LogDebug("Restore backup");

            try
            {
                foreach (var backup in _backups)
                {
                    _logger?.LogDebug($"Restore item: {backup}");
                    if (string.IsNullOrEmpty(backup.Value))
                        _fileUtilities.DeleteFileWithRetry(_fileSystem.FileInfo.New(backup.Key));
                    else
                        _fileUtilities.MoveFile(_fileSystem.FileInfo.New(backup.Value), backup.Key, true);
                }
                return ExternalUpdaterResult.UpdateFailedWithRestore;
            }
            catch (Exception backupException)
            {
                _logger?.LogError($"Error while restoring backup: {backupException.Message}");
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
                    _fileUtilities.DeleteFileWithRetry(_fileSystem.FileInfo.New(item.File));
                if (!string.IsNullOrEmpty(item.Backup))
                    _fileUtilities.DeleteFileWithRetry(_fileSystem.FileInfo.New(item.Backup));
            }
        }
        catch (Exception e)
        {
            _logger?.LogError($"Cleaning failed with error: {e.Message}");
        }
    }
}