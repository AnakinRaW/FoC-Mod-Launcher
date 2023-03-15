using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using AnakinRaW.AppUpdaterFramework.Storage;
using AnakinRaW.AppUpdaterFramework.Updater.Tasks;
using AnakinRaW.CommonUtilities.TaskPipeline;
using AnakinRaW.CommonUtilities.TaskPipeline.Runners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnakinRaW.AppUpdaterFramework.Updater;

internal class UpdateCleanJob : JobBase
{
    private readonly IList<CleanFileTask> _cleanFileTasks;
    private readonly ParallelTaskRunner _taskRunner;
    private readonly IList<string> _filesToBeCleaned = new List<string>();
    private readonly ConcurrentBag<string> _filesFailedToBeCleaned = new();
    private readonly ILogger? _logger;
    private readonly IBackupManager _backupManager;
    private readonly DownloadRepository _downloadRepository;
    private readonly IFileSystem _fileSystem;

    public UpdateCleanJob(IServiceProvider serviceProvider)
    {
        _taskRunner = new ParallelTaskRunner(2, serviceProvider);
        _taskRunner.Error += OnCleaningError;
        _cleanFileTasks = new List<CleanFileTask>();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _backupManager = serviceProvider.GetRequiredService<IBackupManager>();
        _downloadRepository = serviceProvider.GetRequiredService<DownloadRepository>();
    }

    protected override bool PlanCore()
    {
        var files = GetFiles();

            foreach (var data in files)
            {
                var file = data.Value;
                if (!File.Exists(file)) 
                    continue;
                var cleanTask = new CleanFileTask(file);
                _cleanFileTasks.Add(cleanTask);
                _taskRunner.Queue(cleanTask);
                _filesToBeCleaned.Add(file);
            }
        return true;
    }

    protected override void RunCore(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!Plan())
            return;
        if (!_filesToBeCleaned.Any())
            _logger?.LogTrace("No files to clean up");
        else
        {
            _logger?.LogTrace("These files are going to be deleted:");
            foreach (var file in _filesToBeCleaned)
                _logger?.LogTrace(file);

            _taskRunner.Run(token);
            try
            {
                _taskRunner.Wait();
            }
            catch (Exception)
            {
            }

            _backupManager.RemoveBackups();

            ComponentDownloadPathStorage.Instance.Clear();

            if (!_filesFailedToBeCleaned.Any())
                return;
            _logger?.LogTrace("These files have not been deleted because of an internal error:");
            foreach (var file in _filesFailedToBeCleaned)
                _logger?.LogTrace(file);
        }
    }

    private void OnCleaningError(object sender, TaskErrorEventArgs e)
    {
        if (e.Cancel || e.Task is not CleanFileTask task)
            return;
        _filesFailedToBeCleaned.Add(task.File);
    }

    private IEnumerable<KeyValuePair<IComponent, string?>> GetFiles()
    {
        //var backupsFiles = _backupManager.RestoreBackup()
        var downloadFiles = _downloadRepository.GetComponents().Values;
        return backupsFiles.Concat(downloadFiles).ToHashSet();
    }
}