using System;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.AppUpaterFramework.Updater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using FocLauncher.Imaging;
using FocLauncher.Update.Commands;
using FocLauncher.Update.ViewModels.ProductStates;

namespace FocLauncher.Update.ViewModels;

internal class ProductViewModelFactory : IProductViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ProductViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IProductViewModel Create(IInstalledProduct product, IUpdateCatalog? updateCatalog)
    {
        IProductStateViewModel stateViewModel;
        ICommandDefinition? action = null;
        if (updateCatalog is null || updateCatalog.Action == UpdateCatalogAction.None)
        {
            if (product.InstallState == ProductInstallState.RestartPending)
                action = new RestartCommand(_serviceProvider);
            stateViewModel = new InstalledStateViewModel(product, _serviceProvider);
        }
        else if (updateCatalog.Action is UpdateCatalogAction.Install or UpdateCatalogAction.Uninstall)
        {
            stateViewModel = new ErrorStateViewModel(product, "Unable to get update information.", _serviceProvider);
        }
        else
        {
            stateViewModel = new UpdateAvailableStateViewModel(product, updateCatalog, _serviceProvider);
            var isRepair = ProductReferenceEqualityComparer.VersionAware.Equals(product, updateCatalog.Product);
            action = new UpdateCommand(updateCatalog, _serviceProvider, isRepair);
        }

        return new ProductViewModel(product.Name, ImageKeys.AppIcon, stateViewModel, action, _serviceProvider);
    }

    public IProductViewModel Create(IUpdateSession updateSession)
    {
        return new ProductViewModel("Test", ImageKeys.AppIcon, new UpdatingStateViewModel(_serviceProvider), null, _serviceProvider);
    }
}