using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sklavenwalker.ProductMetadata;
using Sklavenwalker.ProductMetadata.Catalog;
using Sklavenwalker.ProductMetadata.Services;

namespace Sklavenwalker.ProductUpdater.Services;

public class ProductUpdateProviderService : IProductUpdateProviderService
{
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

    public async Task<UpdateCheckResult> CheckForUpdates(IProductReference productReference, CancellationToken token = default)
    {
        lock (_syncObject)
        {
            if (IsCheckingForUpdates)
                return UpdateCheckResult.AlreadyInProgress;
            _updateCheckToken = CancellationTokenSource.CreateLinkedTokenSource(token);
        }

        try
        {
            if (productReference.Branch is null)
                throw new CatalogException("Product Reference does not have a branch.");

            var manifestRepo = _serviceProvider.GetRequiredService<IBranchManager>();
            var manifest = await manifestRepo.GetManifest(productReference, _updateCheckToken.Token).ConfigureAwait(false);

            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var installedComponents = productService.GetInstalledProductCatalog();

            var catalogBuilder = _serviceProvider.GetRequiredService<IUpdateCatalogBuilder>();
            var updateCatalog = catalogBuilder.Build(installedComponents, manifest);
            return new UpdateCheckResult(updateCatalog);
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
}