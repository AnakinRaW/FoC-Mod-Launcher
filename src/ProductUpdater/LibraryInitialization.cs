using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductUpdater.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.ProductUpdater;

public static class LibraryInitialization
{
    public static void AddProductUpdater(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddProductMetadata();
        serviceCollection.AddSingleton<IConnectionManager>(_ => new ConnectionManager());
        serviceCollection.AddSingleton<IProductUpdateProviderService>(sp => new ProductUpdateProviderService(sp));
    }
}