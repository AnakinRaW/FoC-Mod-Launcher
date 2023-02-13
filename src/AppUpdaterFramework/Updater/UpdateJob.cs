using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.AppUpaterFramework.Updater.Progress;
using AnakinRaW.AppUpaterFramework.Updater.Tasks;
using AnakinRaW.AppUpaterFramework.Utilities;
using AnakinRaW.CommonUtilities.TaskPipeline;
using AnakinRaW.CommonUtilities.TaskPipeline.Runners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Updater;

internal class UpdateJob : JobBase
{
    private readonly IProgressReporter? _progressReporter;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;

    private readonly HashSet<IUpdateItem> _itemsToProcess;

    private readonly List<IComponentTask> _componentsToInstall = new();
    private readonly List<IComponentTask> _componentsToRemove = new();
    private readonly List<DownloadTask> _componentsToDownload = new();
    private readonly List<IComponentTask> _installsOrRemoves = new();

    private bool _scheduled;
    private ParallelTaskRunner? _downloadsRunner;
    private TaskRunner? _installsRunner;
    private TaskRunner? _cancelCleanupRunner;
    private ReadOnlyCollection<IComponentTask>? _componentsToInstallRo;
    private ReadOnlyCollection<IComponentTask>? _componentsToRemoveRo;
    private ReadOnlyCollection<DownloadTask>? _componentsToDownloadRo;

    private CancellationTokenSource? _linkedCancellationTokenSource;

    internal bool IsCancelled { get; private set; }

    private IReadOnlyCollection<IComponentTask> ComponentsToInstall => 
        _componentsToInstallRo ??= new ReadOnlyCollection<IComponentTask>(_componentsToInstall);

    private IReadOnlyCollection<IComponentTask> ComponentsToRemove =>
        _componentsToRemoveRo ??= new ReadOnlyCollection<IComponentTask>(_componentsToRemove);

    private IReadOnlyCollection<IComponentTask> ComponentsToDownload => 
        _componentsToDownloadRo ??= new ReadOnlyCollection<DownloadTask>(_componentsToDownload);

    public UpdateJob(IUpdateCatalog updateCatalog, IProgressReporter? progressReporter, IServiceProvider serviceProvider)
    {
        Requires.NotNull(updateCatalog, nameof(updateCatalog));
        Requires.NotNull(serviceProvider, nameof(serviceProvider));

        _progressReporter = progressReporter;
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());

        _itemsToProcess = new HashSet<IUpdateItem>(updateCatalog.UpdateItems);
    }

    internal void Schedule()
    {
        if (_scheduled)
            return;
        Initialize();
        if (!Plan())
            return;

        foreach (var d in _componentsToDownload) 
            _downloadsRunner!.Queue(d);
        foreach (var installsOrRemove in _installsOrRemoves) 
            _installsRunner!.Queue(installsOrRemove);

        _scheduled = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        if (_downloadsRunner is null)
        {
            _downloadsRunner = new ParallelTaskRunner(2, _serviceProvider);
            _downloadsRunner.Error += OnError;
        }

        if (_installsRunner is null)
        {
            _installsRunner = new TaskRunner(_serviceProvider);
            _installsRunner.Error += OnError;
        }

        if (_cancelCleanupRunner is null)
        {
            _cancelCleanupRunner = new TaskRunner(_serviceProvider);
            _cancelCleanupRunner.Error += OnError;
        }
    }

    protected override bool PlanCore()
    {
        if (_itemsToProcess.Count == 0)
        {
            var ex = new InvalidOperationException("No items to update/remove.");
            _logger?.LogError(ex, ex.Message);
            return false;
        }
        
        foreach (var updateItem in _itemsToProcess)
        {
            var installedComponent = updateItem.InstalledComponent;
            var updateComponent = updateItem.UpdateComponent;
            if (updateItem.Action == UpdateAction.Update && updateComponent != null)
            {
                if (updateComponent.OriginInfo is null)
                    throw new InvalidOperationException($"OriginInfo is missing for '{updateComponent}'");
                
                var installTask = new InstallTask(updateComponent, UpdateAction.Update, _serviceProvider);
                var downloadTask = new DownloadTask(updateComponent, _serviceProvider);

                _componentsToInstall.Add(installTask);
                _componentsToDownload.Add(downloadTask);
            }

            if (updateItem.Action == UpdateAction.Delete && installedComponent != null)
            {
                var removeTask = new InstallTask(installedComponent, UpdateAction.Delete, _serviceProvider);
                _componentsToRemove.Add(removeTask);
            }
        }

        _componentsToRemove.Reverse();
        _installsOrRemoves.Clear();
        _installsOrRemoves.AddRange(_componentsToRemove.Concat(_componentsToInstall));

        return true;
    }

    protected override void RunCore(CancellationToken token)
    {
        Schedule();
        var installs = _installsOrRemoves.OfType<InstallTask>().ToList();
            

        _progressReporter?.Report("Starting update...", 0.0, ProgressType.Install, new ProgressInfo());

        if (ComponentsToDownload.Any())
        {
            // TODO: Progress
        }
        else
            _progressReporter?.Report("_", 1.0, ProgressType.Download, new ProgressInfo());

        if (installs.Any())
        {
            // TODO: Progress
        }
        else
            _progressReporter?.Report("_", 1.0, ProgressType.Install, new ProgressInfo());

        
        try
        {
            _logger?.LogTrace("Starting update job.");
            _linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            _downloadsRunner!.Run(_linkedCancellationTokenSource.Token);
#if DEBUG
            _downloadsRunner.Wait();
#endif
            _installsRunner!.Run(_linkedCancellationTokenSource.Token);
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

        var failedDownloads = _componentsToDownload.Where(p =>
            p.Error != null && !p.Error.IsExceptionType<OperationCanceledException>());

        var failedInstalls = installs
            .Where(installTask => !installTask.Result.IsSuccess()).ToList();

        if (failedDownloads.Any() || failedInstalls.Any())
            throw new ComponentFailedException(
                "Update failed because one or more downloads or installs had an error.");

        //var requiresRestart = LockedFilesWatcher.Instance.LockedFiles.Any();
        //if (requiresRestart)
        //    _logger?.LogInformation("The operation finished. A restart is pedning.");

        Task.Delay(5000, token).Wait(token);
    }

    private void OnError(object sender, TaskErrorEventArgs e)
    {
        IsCancelled |= e.Cancel;
        if (e.Cancel && _linkedCancellationTokenSource is not null) 
            _linkedCancellationTokenSource.Cancel();
        try
        {
            if (e.Cancel || e.Task is not IComponentTask)
                return;
            if (e.Task is InstallTask)
            {
                // TODO
            }
            if (e.Task is DownloadTask)
            {
                // TODO
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, ex.Message);
        }
    }
}