using AnakinRaW.ApplicationBase.Update;
using AnakinRaW.ApplicationBase.Update.Manifest;
using AnakinRaW.AppUpdaterFramework.Configuration;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.AppUpdaterFramework.Product.Manifest;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.ApplicationBase;

public static class LibraryInitialization
{
    public static void AddApplicationBase(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IProductService>(sp => new ApplicationProductService(sp));
        serviceCollection.AddSingleton<IBranchManager>(sp => new ApplicationBranchManager(sp));
        serviceCollection.AddSingleton<IManifestLoader>(sp => new ManifestLoader(sp));
        serviceCollection.AddSingleton<IUpdateConfigurationProvider>(sp => new ApplicationUpdateConfigurationProvider(sp));
        serviceCollection.AddSingleton<IInstalledManifestProvider>(sp => new ApplicationInstalledManifestProvider(sp));
    }
}