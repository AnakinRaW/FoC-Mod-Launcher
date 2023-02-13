using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.AppUpaterFramework.Product;
using AnakinRaW.AppUpaterFramework.Utilities;
using AnakinRaW.CommonUtilities.TaskPipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Updater;

public class UpdateService : IUpdateService
{
    public event EventHandler? CheckingForUpdatesStarted;
    public event EventHandler<IUpdateCatalog?>? CheckingForUpdatesCompleted;
    public event EventHandler<IUpdateSession>? UpdateStarted;
    public event EventHandler? UpdateCompleted;

    private readonly IServiceProvider _serviceProvider;
    private readonly object _syncObject = new();
    private CancellationTokenSource? _updateCheckToken;
    private readonly ILogger? _logger;

    public bool IsCheckingForUpdates
    {
        get
        {
            lock (_syncObject)
                return _updateCheckToken != null;
        }
    }

    public UpdateService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public async Task CheckForUpdates(IProductReference productReference, CancellationToken token = default)
    {
        lock (_syncObject)
        {
            if (IsCheckingForUpdates)
                return;
            _updateCheckToken = CancellationTokenSource.CreateLinkedTokenSource(token);
        }

        CheckingForUpdatesStarted?.RaiseAsync(this, EventArgs.Empty);

        try
        {
            IUpdateCatalog? updateCatalog = null;
            try
            {
                if (productReference.Branch is null)
                    throw new CatalogException("Product Reference does not have a branch.");
                
                var manifestRepo = _serviceProvider.GetRequiredService<IBranchManager>();
                var manifest = await manifestRepo.GetManifest(productReference, _updateCheckToken.Token).ConfigureAwait(false);

                var productService = _serviceProvider.GetRequiredService<IProductService>();
                var installedComponents = productService.GetInstalledComponents();
                var currentInstance = productService.GetCurrentInstance();

                var updateCatalogBuilder = _serviceProvider.GetRequiredService<IUpdateCatalogProvider>();
                updateCatalog = updateCatalogBuilder.Create(currentInstance, installedComponents, manifest);
            }
            finally
            {
                CheckingForUpdatesCompleted?.RaiseAsync(this, updateCatalog);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex.Message);
            throw;
        }
        finally
        {
            lock (_syncObject)
            {
                _updateCheckToken.Dispose();
                _updateCheckToken = null;
            }
        }
    }

    public async Task<object> Update(IUpdateCatalog updateCatalog)
    {
        var updater = CreateUpdater(updateCatalog);
        var updateSession = new UpdateSession(updateCatalog.UpdateReference, updater);
        try
        {
            UpdateStarted?.RaiseAsync(this, updateSession);
            return await updateSession.StartUpdate();
        }
        finally
        {
            UpdateCompleted?.RaiseAsync(this, EventArgs.Empty);
        }
    }


    private IApplicationUpdater CreateUpdater(IUpdateCatalog updateCatalog)
    {
        return new ApplicationUpdater(updateCatalog, _serviceProvider);
    }
}

internal class ApplicationUpdater : IApplicationUpdater, IProgressReporter
{
    private readonly IUpdateCatalog _updateCatalog;
    private readonly IServiceProvider _serviceProvider;
    public event EventHandler<ProgressEventArgs?>? Progress;

    private readonly ILogger? _logger;

    public ApplicationUpdater(IUpdateCatalog updateCatalog, IServiceProvider serviceProvider)
    {
        Requires.NotNull(updateCatalog, nameof(updateCatalog));
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _updateCatalog = updateCatalog;
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public void Report(string package, double progress, ProgressType type, ProgressInfo detailedProgress)
    {
    }

    public async Task<object> UpdateAsync(CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            await UpdateCoreAsync(token);
            return null;
        }
        catch (Exception e)
        {
            _logger?.LogError($"Update failed: {e.Message}");
            if (ShouldRethrowEngineException(e)) 
                ExceptionDispatchInfo.Capture(e).Throw();

            return null; // TODO
        }
    }

    private async Task UpdateCoreAsync(CancellationToken token)
    {
        try
        {
            if (!_updateCatalog.UpdateItems.Any())
                throw new InvalidOperationException("Nothing to update!");

            await Task.Run(() =>
            {
                var updateJob = new UpdateJob(_updateCatalog, this, _serviceProvider);
                updateJob.Schedule();
                _logger?.LogTrace("Starting update");
                updateJob.Run(token);
            }, CancellationToken.None).ConfigureAwait(false);

        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, ex.Message);
            throw;
        }
    }

    private static bool ShouldRethrowEngineException(Exception ex)
    {
        return ex is not ComponentFailedException && !ex.IsOperationCanceledException();
    }
}

internal interface IApplicationUpdater
{
    event EventHandler<ProgressEventArgs?> Progress;

    Task<object> UpdateAsync(CancellationToken token);
}


internal class UpdateJob : JobBase
{
    private readonly IUpdateCatalog _updateCatalog;
    private readonly IProgressReporter? _progressReporter;
    private readonly IServiceProvider _serviceProvider;

    public UpdateJob(IUpdateCatalog updateCatalog, IProgressReporter? progressReporter, IServiceProvider serviceProvider)
    {
        Requires.NotNull(updateCatalog, nameof(updateCatalog));
        Requires.NotNull(serviceProvider, nameof(serviceProvider));

        _updateCatalog = updateCatalog;
        _progressReporter = progressReporter;
        _serviceProvider = serviceProvider;
    }

    public void Schedule()
    {

    }

    protected override bool PlanCore()
    {
        return true;
    }

    protected override void RunCore(CancellationToken token)
    {
        Task.Delay(5000, token).Wait(token);
    }
}

internal interface IProgressReporter
{
    void Report(string package, double progress, ProgressType type, ProgressInfo detailedProgress);
}



//internal class PollingProgressReporter : IDisposable
//{
//    public event EventHandler<ProgressEventArgs>? OnReport;

//    private Timer? _timer;
//    private bool _disposed;

//    public PollingProgressReporter() : this(500.0)
//    {
//    }

//    public PollingProgressReporter(double pollingRate)
//    {
        
//    }

//    public void Report(string package, double progress, ProgressType type, ProgressInfo detailedProgress)
//    {

//    }

//    public void Dispose()
//    {
//        Dispose(true);
//        GC.SuppressFinalize(this);
//    }

//    private void Dispose(bool disposing)
//    {
//        if (_disposed)
//            return;
//        if (disposing && _timer != null)
//            _timer.Dispose();
//        _disposed = true;
//    }
//}