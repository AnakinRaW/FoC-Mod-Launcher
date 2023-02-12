using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.AppUpaterFramework.Product;
using AnakinRaW.AppUpaterFramework.Utilities;
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
                var variables = productService.GetCurrentInstance().Variables;

                var updateCatalogBuilder = _serviceProvider.GetRequiredService<IUpdateCatalogProvider>();
                updateCatalog = updateCatalogBuilder.Create(installedComponents, manifest, variables);
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
        var updateSession = new UpdateSession(updateCatalog.Product, updater);
        try
        {
            UpdateStarted?.RaiseAsync(this, updateSession);
            return await updateSession.StartUpdate();
        }
        finally
        {
            UpdateCompleted?.RaiseAsync(this, EventArgs.Empty);
            updater.Dispose();
        }
    }


    private IApplicationUpdater CreateUpdater(IUpdateCatalog updateCatalog)
    {
        return new ApplicationUpdater();
    }
}

internal class ApplicationUpdater : IApplicationUpdater
{
    public event EventHandler<ProgressEventArgs?>? Progress;

    public async Task<object> UpdateAsync(CancellationToken token)
    {
        for (int i = 0; i < 100; i++)
        {
            Progress?.Invoke(this, new ProgressEventArgs("C", (double) i / 100, ProgressType.Install));
            await Task.Delay(50, token);
        }

        return null;
    }

    public void Dispose()
    {
    }
}

internal interface IApplicationUpdater : IDisposable
{
    event EventHandler<ProgressEventArgs?> Progress;

    Task<object> UpdateAsync(CancellationToken token);
}

public class ProgressEventArgs : EventArgs
{
    public string Component { get; }

    public double Progress { get; }

    public ProgressType Type { get; }

    public ProgressInfo DetailedProgress { get; }

    public ProgressEventArgs(string component, double progress, ProgressType type)
        : this(component, progress, type, new ProgressInfo())
    {
    }

    public ProgressEventArgs(string component, double progress, ProgressType type, ProgressInfo detailedProgress)
    {
        Requires.NotNullOrEmpty(component, nameof(component));
        Component = component;
        Progress = progress;
        Type = type;
        DetailedProgress = detailedProgress;
    }
}

public enum ProgressType
{
    None = -1,
    Install = 0,
    Download = 1,
    Verify = 2,
    Clean = 3
}

public struct ProgressInfo
{
    public ProgressInfo(int currentComponent, int totalComponents, long downloadedSize, long totalSize, long downloadSpeed)
    {
        CurrentComponent = currentComponent;
        TotalComponents = totalComponents;
        DownloadedSize = downloadedSize;
        TotalSize = totalSize;
        DownloadSpeed = downloadSpeed;
    }

    public int CurrentComponent { get; internal set; }

    public int TotalComponents { get; internal set; }

    public long DownloadedSize { get; internal set; }

    public long TotalSize { get; internal set; }

    public long DownloadSpeed { get; internal set; }

    public override string ToString() =>
        $"Package={CurrentComponent},TotalComponents={TotalComponents},DownloadedSize={DownloadedSize},Total={TotalSize},DownloadSpeed={DownloadSpeed}";
}