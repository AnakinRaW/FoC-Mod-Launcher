using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Updater.Configuration;
using AnakinRaW.AppUpdaterFramework.Updater.Progress;
using AnakinRaW.AppUpdaterFramework.Updater.Tasks;
using AnakinRaW.AppUpdaterFramework.Utilities;
using AnakinRaW.CommonUtilities.TaskPipeline;
using AnakinRaW.CommonUtilities.TaskPipeline.Runners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Updater;

internal sealed class UpdateJob : JobBase, IDisposable
{
    private bool _disposed;
    private CancellationTokenSource? _linkedCancellationTokenSource;

    private readonly IProgressReporter _progressReporter;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;
    private readonly IInstalledProduct _installedProduct;

    private readonly HashSet<IUpdateItem> _itemsToProcess;

    private readonly AggregatedComponentProgressReporter _installProgress;
    private readonly AggregatedComponentProgressReporter _downloadProgress;

    private readonly List<DownloadTask> _componentsToDownload = new();
    private readonly List<InstallTask> _installsOrRemoves = new();

    private readonly ParallelTaskRunner _downloadsRunner;
    private readonly TaskRunner _installsRunner;

    private bool IsCancelled { get; set; }

    public UpdateJob(IUpdateCatalog updateCatalog, IProgressReporter progressReporter, IServiceProvider serviceProvider)
    {
        Requires.NotNull(updateCatalog, nameof(updateCatalog));
        Requires.NotNull(serviceProvider, nameof(serviceProvider));

        _progressReporter = progressReporter;
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _installedProduct = updateCatalog.InstalledProduct;

        _itemsToProcess = new HashSet<IUpdateItem>(updateCatalog.UpdateItems);

        _installsRunner = new TaskRunner(_serviceProvider);
        _installProgress = new AggregatedInstallProgressReporter(progressReporter);
        _downloadsRunner = new ParallelTaskRunner(2, _serviceProvider);
        _downloadProgress = new AggregatedDownloadProgressReporter(progressReporter);

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _downloadsRunner.Error += OnError;
        _installsRunner.Error += OnError;
    }

    private void UnregisterEvents()
    {
        _downloadsRunner.Error -= OnError;
        _installsRunner.Error -= OnError;
    }

    protected override bool PlanCore()
    {
        if (_disposed)
            throw new ObjectDisposedException("Job already disposed");

        if (_itemsToProcess.Count == 0)
        {
            var ex = new InvalidOperationException("No items to update/remove.");
            _logger?.LogError(ex, ex.Message);
            return false;
        }

        _componentsToDownload.Clear();
        _installsOrRemoves.Clear();

        var configuration = _serviceProvider.GetService<IUpdateConfigurationProvider>()?.GetConfiguration() ??
                            UpdateConfiguration.Default;

        foreach (var updateItem in _itemsToProcess)
        {
            var installedComponent = updateItem.InstalledComponent;
            var updateComponent = updateItem.UpdateComponent;
            if (updateItem.Action == UpdateAction.Update && updateComponent != null)
            {
                if (updateComponent.OriginInfo is null)
                    throw new InvalidOperationException($"OriginInfo is missing for '{updateComponent}'");
                
                var downloadTask = new DownloadTask(updateComponent, _downloadProgress, configuration, _serviceProvider);
                var installTask = new InstallTask(updateComponent, installedComponent, downloadTask, _installProgress, configuration, _installedProduct.Variables, _serviceProvider);

                _installsOrRemoves.Add(installTask);
                _componentsToDownload.Add(downloadTask);
            }

            if (updateItem.Action == UpdateAction.Delete && installedComponent != null)
            {
                var removeTask = new InstallTask(installedComponent, _installProgress, configuration, _installedProduct.Variables, _serviceProvider);
                _installsOrRemoves.Add(removeTask);
            }
        }

        foreach (var d in _componentsToDownload)
            _downloadsRunner.Queue(d);
        foreach (var installsOrRemove in _installsOrRemoves)
            _installsRunner.Queue(installsOrRemove);

        return true;
    }

    protected override void RunCore(CancellationToken token)
    {
        if (_disposed)
            throw new ObjectDisposedException("Job already disposed");

        _progressReporter.Report("Starting update...", 0.0, ProgressType.Install, new ProgressInfo());

        var componentsToDownload = _componentsToDownload.ToList();
        var componentsToInstallOrRemove = _installsOrRemoves.ToList();

        if (!componentsToDownload.Any())
            _progressReporter.Report("_", 1.0, ProgressType.Download, new ProgressInfo());
        else
            _downloadProgress.Initialize(componentsToDownload);

        if (!componentsToInstallOrRemove.Any())
            _progressReporter.Report("_", 1.0, ProgressType.Install, new ProgressInfo());
        else
            _installProgress.Initialize(componentsToInstallOrRemove); 
        
        try
        {
            _logger?.LogTrace("Starting update job.");
            _linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            _downloadsRunner.Run(_linkedCancellationTokenSource.Token);
#if DEBUG
            _downloadsRunner.Wait();
#endif
            _installsRunner.Run(_linkedCancellationTokenSource.Token);
            try
            {
                _downloadsRunner.Wait();
            }
            catch
            {
                // Ignore
            }
        }
        finally
        {
            if (_linkedCancellationTokenSource is not null)
            {
                _linkedCancellationTokenSource.Dispose();
                _linkedCancellationTokenSource = null;
            }
            _logger?.LogTrace("Completed update job.");
        }

        if (IsCancelled)
            throw new OperationCanceledException(token);
        token.ThrowIfCancellationRequested();

        var failedDownloads = componentsToDownload.Where(p =>
            p.Error != null && !p.Error.IsExceptionType<OperationCanceledException>());

        var failedInstalls = componentsToInstallOrRemove
            .Where(installTask => !installTask.Result.IsSuccess()).ToList();

        var failedTasks = failedDownloads.Concat<IProgressTask>(failedInstalls).ToList();
        
        if (failedTasks.Any() || failedInstalls.Any())
            throw new ComponentFailedException(failedTasks);


        //var requiresRestart = LockedFilesWatcher.Instance.LockedFiles.Any();
        //if (requiresRestart)
        //    _logger?.LogInformation("The operation finished. A restart is pending.");
    }

    private void OnError(object sender, TaskErrorEventArgs e)
    {
        IsCancelled |= e.Cancel;
        if (e.Cancel && _linkedCancellationTokenSource is not null) 
            _linkedCancellationTokenSource.Cancel();
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        UnregisterEvents();
        _disposed = true;
    }
}