using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AnakinRaW.AppUpdaterFramework.Configuration;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Restart;
using AnakinRaW.AppUpdaterFramework.Updater.Progress;
using AnakinRaW.AppUpdaterFramework.Updater.Tasks;
using AnakinRaW.AppUpdaterFramework.Utilities;
using AnakinRaW.CommonUtilities.SimplePipeline;
using AnakinRaW.CommonUtilities.SimplePipeline.Runners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Updater;

internal sealed class UpdatePipeline : Pipeline
{
    private CancellationTokenSource? _linkedCancellationTokenSource;

    private readonly IComponentProgressReporter _progressReporter;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;
    private readonly IInstalledProduct _installedProduct;

    private readonly HashSet<IUpdateItem> _itemsToProcess;

    private readonly ComponentAggregatedProgressReporter _installProgress;
    private readonly ComponentAggregatedProgressReporter _downloadProgress;

    private readonly List<DownloadStep> _componentsToDownload = new();
    private readonly List<InstallStep> _installsOrRemoves = new();

    private readonly ParallelRunner _downloadsRunner;
    private readonly StepRunner _installsRunner;
    private readonly IRestartManager _restartManager;

    private bool IsCancelled { get; set; }

    public UpdatePipeline(IUpdateCatalog updateCatalog, IComponentProgressReporter progressReporter, IServiceProvider serviceProvider)
    {
        Requires.NotNull(updateCatalog, nameof(updateCatalog));
        Requires.NotNull(serviceProvider, nameof(serviceProvider));

        _progressReporter = progressReporter;
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _installedProduct = updateCatalog.InstalledProduct;
        _restartManager = serviceProvider.GetRequiredService<IRestartManager>();

        _itemsToProcess = new HashSet<IUpdateItem>(updateCatalog.UpdateItems);

        _installsRunner = new StepRunner(_serviceProvider);
        _installProgress = new AggregatedInstallProgressReporter(progressReporter);
        _downloadsRunner = new ParallelRunner(2, _serviceProvider);
        _downloadProgress = new AggregatedDownloadProgressReporter(progressReporter);

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _downloadsRunner.Error += OnError!;
        _installsRunner.Error += OnError!;
        _restartManager.RestartRequired += OnRestartRequired;
    }

    private void UnregisterEvents()
    {
        _downloadsRunner.Error -= OnError!;
        _installsRunner.Error -= OnError!;
        _restartManager.RestartRequired -= OnRestartRequired;
    }

    protected override bool PrepareCore()
    { 
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
                
                var downloadTask = new DownloadStep(updateComponent, _downloadProgress, configuration, _serviceProvider);
                var installTask = new InstallStep(updateComponent, installedComponent, downloadTask, _installProgress, configuration, _installedProduct.Variables, _serviceProvider);

                _installsOrRemoves.Add(installTask);
                _componentsToDownload.Add(downloadTask);
            }

            if (updateItem.Action == UpdateAction.Delete && installedComponent != null)
            {
                var removeTask = new InstallStep(installedComponent, _installProgress, configuration, _installedProduct.Variables, _serviceProvider);
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
        _progressReporter.Report("Starting update...", 0.0, ProgressTypes.Install, new ComponentProgressInfo());

        var componentsToDownload = _componentsToDownload.ToList();
        var componentsToInstallOrRemove = _installsOrRemoves.ToList();

        if (!componentsToDownload.Any())
            _progressReporter.Report("_", 1.0, ProgressTypes.Download, new ComponentProgressInfo());
        else
            _downloadProgress.Initialize(componentsToDownload);

        if (!componentsToInstallOrRemove.Any())
            _progressReporter.Report("_", 1.0, ProgressTypes.Install, new ComponentProgressInfo());
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

        if (_restartManager.RequiredRestartType == RestartType.ApplicationElevation)
            throw new ElevationRequireException();

        if (IsCancelled)
            throw new OperationCanceledException(token);
        token.ThrowIfCancellationRequested();

        var failedDownloads = componentsToDownload.Where(p =>
            p.Error != null && !p.Error.IsExceptionType<OperationCanceledException>());

        var failedInstalls = componentsToInstallOrRemove
            .Where(installTask => !installTask.Result.IsSuccess()).ToList();

        var failedTasks = failedDownloads.Concat<IProgressStep>(failedInstalls).ToList();
        
        if (failedTasks.Any() || failedInstalls.Any())
            throw new StepFailureException(failedTasks);
    }

    private void OnError(object sender, StepErrorEventArgs e)
    {
        IsCancelled |= e.Cancel;
        if (e.Cancel && _linkedCancellationTokenSource is not null) 
            _linkedCancellationTokenSource.Cancel();
    }

    protected override void DisposeManagedResources()
    {
        base.DisposeManagedResources();
        UnregisterEvents();
    }

    private void OnRestartRequired(object? sender, EventArgs e)
    {
        if (_restartManager.RequiredRestartType != RestartType.ApplicationElevation)
            return;

        _logger?.LogWarning("Elevation requested. Update gets cancelled");
        _linkedCancellationTokenSource?.Cancel();
        _restartManager.RestartRequired -= OnRestartRequired;
    }
}