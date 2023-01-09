using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductUpdater.Catalog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnakinRaW.ProductUpdater.Services;

public class ProductUpdateProviderService : IProductUpdateProviderService
{
    public event EventHandler? CheckingForUpdatesStarted;
    public event EventHandler<IUpdateCatalog?>? CheckingForUpdatesCompleted;

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

    public ProductUpdateProviderService(IServiceProvider serviceProvider)
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

        CheckingForUpdatesStarted.RaiseAsync(this, EventArgs.Empty);

        await Task.Delay(2000, token);

        CheckingForUpdatesCompleted.RaiseAsync(this, null);

        //try
        //{
        //    IUpdateCatalog? updateCatalog = null;
        //    try
        //    {
        //        if (productReference.Branch is null)
        //            throw new CatalogException("Product Reference does not have a branch.");

        //        var manifestRepo = _serviceProvider.GetRequiredService<IBranchManager>();
        //        var manifest = await manifestRepo.GetManifest(productReference, _updateCheckToken.Token).ConfigureAwait(false);

        //        var productService = _serviceProvider.GetRequiredService<IProductService>();
        //        var installedComponents = productService.GetInstalledProductCatalog();

        //        var catalogBuilder = _serviceProvider.GetRequiredService<IUpdateCatalogBuilder>();
        //        updateCatalog = catalogBuilder.Build(installedComponents, manifest);
        //    }
        //    finally
        //    {
        //        CheckingForUpdatesCompleted.RaiseAsync(this, updateCatalog);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    _logger?.LogWarning(ex.Message);
        //    throw;
        //}
        //finally
        //{
        //    lock (_syncObject)
        //    {
        //        _updateCheckToken.Dispose();
        //        _updateCheckToken = null;
        //    }
        //}
    }
}