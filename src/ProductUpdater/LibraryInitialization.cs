using AnakinRaW.AppUpaterFramework.Services;
using AnakinRaW.ProductMetadata;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.AppUpaterFramework;

public static class LibraryInitialization
{
    public static void AddProductUpdater(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddProductMetadata();
        serviceCollection.AddSingleton<IConnectionManager>(_ => new ConnectionManager());
        serviceCollection.AddSingleton<IProductUpdateProviderService>(sp => new ProductUpdateProviderService(sp));
        serviceCollection.AddSingleton<IUpdateCatalogProvider>(sp => new UpdateCatalogProvider(sp));
    }
}