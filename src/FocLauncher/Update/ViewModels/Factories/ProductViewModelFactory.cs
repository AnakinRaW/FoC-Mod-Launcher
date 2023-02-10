using System;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using FocLauncher.Imaging;
using FocLauncher.Update.Commands;
using FocLauncher.Update.ViewModels.ProductStates;

namespace FocLauncher.Update.ViewModels;

internal class ProductViewModelFactory : IProductViewModelFactory
{
    public IProductViewModel Create(IInstalledProduct product, IUpdateCatalog? updateCatalog, IServiceProvider serviceProvider)
    {
        IProductStateViewModel stateViewModel;
        ICommandDefinition? action = null;
        if (updateCatalog is null || updateCatalog.Action == UpdateCatalogAction.None)
        {
            if (product.InstallState == ProductInstallState.RestartPending)
                action = new RestartCommand(serviceProvider);
            stateViewModel = new InstalledStateViewModel(product, serviceProvider);
        }
        else if (updateCatalog.Action is UpdateCatalogAction.Install or UpdateCatalogAction.Uninstall)
        {
            stateViewModel = new ErrorStateViewModel(product, "Unable to get update information.", serviceProvider);
        }
        else
        {
            stateViewModel = new UpdateAvailableStateViewModel(product, updateCatalog, serviceProvider);
            var isRepair = ProductReferenceEqualityComparer.VersionAware.Equals(product, updateCatalog.Product);
            action = new UpdateCommand(updateCatalog, serviceProvider, isRepair);
        }

        return new ProductViewModel(product.Name, ImageKeys.AppIcon, stateViewModel, action, serviceProvider);
    }
}