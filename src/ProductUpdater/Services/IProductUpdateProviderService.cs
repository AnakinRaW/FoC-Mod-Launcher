using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductUpdater.Catalog;

namespace AnakinRaW.ProductUpdater.Services;

public interface IProductUpdateProviderService
{
    event EventHandler CheckingForUpdatesStarted;

    event EventHandler<IUpdateCatalog?> CheckingForUpdatesCompleted;

    bool IsCheckingForUpdates { get; }

    Task CheckForUpdates(IProductReference productReference, CancellationToken token = default);
}