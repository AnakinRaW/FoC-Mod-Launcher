using AnakinRaW.ProductMetadata.Services.Detectors;
using AnakinRaW.ProductUpdater.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.ProductUpdater;

public static class LibraryInitialization
{
    public static void AddProductUpdater(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IConnectionManager>(_ => new ConnectionManager());
        serviceCollection.AddSingleton<IProductUpdateProviderService>(sp => new ProductUpdateProviderService(sp));
        serviceCollection.AddSingleton<ICatalogDetectionService>(sp => new CatalogDetectionService(sp));
    }
}