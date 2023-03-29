using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Storage;
using AnakinRaW.CommonUtilities.TaskPipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnakinRaW.AppUpdaterFramework.Updater;

internal class UpdateCleanJob : JobBase
{
    private readonly List<IInstallableComponent> _filesFailedToBeCleaned = new();
    private readonly ILogger? _logger;
    private readonly IBackupManager _backupManager;
    private readonly DownloadRepository _downloadRepository;
    private readonly List<IInstallableComponent> _downloadsToClean = new();
    private readonly List<IInstallableComponent> _backupsToClean = new();

    public UpdateCleanJob(IServiceProvider serviceProvider)
    { 
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _backupManager = serviceProvider.GetRequiredService<IBackupManager>();
        _downloadRepository = serviceProvider.GetRequiredService<DownloadRepository>();
    }

    protected override bool PlanCore()
    {
        _backupsToClean.Clear();
        _downloadsToClean.Clear();

        _backupsToClean.AddRange(_backupManager.Backups.Keys);
        _downloadsToClean.AddRange(_downloadRepository.GetComponents().Keys);
        return true;
    }

    protected override void RunCore(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!Plan())
            return;
        if (!_downloadsToClean.Any() && !_backupsToClean.Any())
        {
            _logger?.LogTrace("No files to clean up");
            return;
        }

        _filesFailedToBeCleaned.Clear();
        
        foreach (var backup in _backupsToClean) 
            GuardedClean(backup, _backupManager.RemoveBackup);

        foreach (var download in _downloadsToClean)
            GuardedClean(download, _downloadRepository.RemoveComponent);


        if (!_filesFailedToBeCleaned.Any())
            return;
        _logger?.LogTrace("These components could not be deleted:");
        foreach (var file in _filesFailedToBeCleaned)
            _logger?.LogTrace(file.GetDisplayName());
    }

    private void GuardedClean(IInstallableComponent component,  Action<IInstallableComponent> cleanOperation)
    {
        try
        {
            cleanOperation(component);
        }
        catch (Exception)
        {
            _filesFailedToBeCleaned.Add(component);
        }
    }
}