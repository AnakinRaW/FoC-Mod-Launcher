using System;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using FocLauncher.Imaging;
using FocLauncher.Update.Commands;
using FocLauncher.Update.ViewModels.ProductStates;

namespace FocLauncher.Update.ViewModels;

internal class InstalledProductViewModelFactory : IInstalledProductViewModelFactory
{
    public IInstalledProductViewModel Create(IInstalledProduct product, IUpdateCatalog? updateCatalog, IServiceProvider serviceProvider)
    {
        IInstalledProductStateViewModel stateViewModel;
        ICommandDefinition? action = null;
        if (updateCatalog is null || updateCatalog.Action == UpdateCatalogAction.None)
        {
            if (product.InstallState == ProductInstallState.RestartPending)
                action = new RestartCommand(serviceProvider);
            stateViewModel = new InstalledStateProductViewModel(product, serviceProvider);
        }
        else if (updateCatalog.Action is UpdateCatalogAction.Install or UpdateCatalogAction.Uninstall)
        {
            stateViewModel = new ErrorStateProductViewModel(product, "Unable to get update information.", serviceProvider);
        }
        else
        {
            stateViewModel = new UpdateAvailableStateProductViewModel(product, updateCatalog, serviceProvider);
            var isRepair = ProductReferenceEqualityComparer.VersionAware.Equals(product, updateCatalog.Product);
            action = new UpdateCommand(serviceProvider, isRepair);
        }

        return new InstalledProductViewModel(product.Name, ImageKeys.AppIcon, stateViewModel, action, serviceProvider);
    }
}