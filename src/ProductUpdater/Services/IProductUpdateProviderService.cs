using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Catalog;
using AnakinRaW.ProductMetadata;

namespace AnakinRaW.AppUpaterFramework.Services;

public interface IProductUpdateProviderService
{
    event EventHandler CheckingForUpdatesStarted;

    event EventHandler<IUpdateCatalog?> CheckingForUpdatesCompleted;

    bool IsCheckingForUpdates { get; }

    Task CheckForUpdates(IProductReference productReference, CancellationToken token = default);
}