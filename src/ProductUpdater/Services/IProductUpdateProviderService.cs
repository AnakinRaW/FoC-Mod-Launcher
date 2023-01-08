using System;
using System.Threading;
using System.Threading.Tasks;
using Sklavenwalker.ProductMetadata;
using Sklavenwalker.ProductUpdater.Catalog;

namespace Sklavenwalker.ProductUpdater.Services;

public interface IProductUpdateProviderService
{
    event EventHandler CheckingForUpdatesStarted;

    event EventHandler<IUpdateCatalog?> CheckingForUpdatesCompleted;

    bool IsCheckingForUpdates { get; }

    Task CheckForUpdates(IProductReference productReference, CancellationToken token = default);
}