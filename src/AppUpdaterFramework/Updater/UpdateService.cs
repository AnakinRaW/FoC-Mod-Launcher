using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.AppUpdaterFramework.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnakinRaW.AppUpdaterFramework.Updater;

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

    public async Task<UpdateResult> Update(IUpdateCatalog updateCatalog)
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