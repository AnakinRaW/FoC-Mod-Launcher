using AnakinRaW.ApplicationBase.Commands.Handlers;
using AnakinRaW.ApplicationBase.Imaging;
using AnakinRaW.ApplicationBase.Services;
using AnakinRaW.ApplicationBase.Update.ApplicationImplementations;
using AnakinRaW.ApplicationBase.Update.Manifest;
using AnakinRaW.AppUpdaterFramework;
using AnakinRaW.AppUpdaterFramework.Configuration;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.AppUpdaterFramework.Product.Manifest;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnakinRaW.ApplicationBase;

public static class LibraryInitialization
{
    public static void AddApplicationBase(this IServiceCollection serviceCollection, ImageKey applicationIcon = default)
    {
        serviceCollection.AddUpdateGui(applicationIcon);

        serviceCollection.AddSingleton<IShowUpdateWindowCommandHandler>(sp => new ShowUpdateWindowCommandHandler(sp));

        serviceCollection.AddSingleton<IProductService>(sp => new ApplicationProductService(sp));
        serviceCollection.AddSingleton<IBranchManager>(sp => new ApplicationBranchManager(sp));
        serviceCollection.AddSingleton<IManifestLoader>(sp => new ManifestLoader(sp));
        serviceCollection.AddSingleton<IUpdateConfigurationProvider>(sp => new ApplicationUpdateConfigurationProvider(sp));
        serviceCollection.AddSingleton<IInstalledManifestProvider>(sp => new ApplicationInstalledManifestProvider(sp));

        serviceCollection.AddSingleton(sp => new ApplicationUpdateInteractionFactory(sp));
        serviceCollection.AddSingleton<IUpdateDialogViewModelFactory>(sp => sp.GetRequiredService<ApplicationUpdateInteractionFactory>());

        serviceCollection.TryAddSingleton<IModalWindowFactory>(sp => new ApplicationModalWindowFactory(sp));
        serviceCollection.TryAddSingleton<IDialogFactory>(sp => new ApplicationDialogFactory(sp));
        
        ImageLibrary.Instance.LoadCatalog(ImageCatalog.Instance);
    }
}