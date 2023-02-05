using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductUpdater.Catalog;
using FocLauncher.Commands;
using FocLauncher.Imaging;
using FocLauncher.Update.Commands;

namespace FocLauncher.Update.ViewModels;

internal class InstalledProductViewModelFactory : IInstalledProductViewModelFactory
{
    public IInstalledProductViewModel Create(IInstalledProduct product, IUpdateCatalog? updateCatalog, IServiceProvider serviceProvider)
    {
        IInstalledProductStateViewModel stateViewModel;
        ICommandDefinition? action = null;
        if (updateCatalog is null)
        {
            if (product.InstallState == ProductInstallState.RestartPending)
                action = new RestartCommand(serviceProvider);
            stateViewModel = new InstalledStateProductViewModel(product, serviceProvider);
        }
        else
        {
            stateViewModel = new UpdateAvailableStateProductViewModel(product, updateCatalog, serviceProvider);
            action = new UpdateCommand(serviceProvider);
        }

        return new InstalledProductViewModel(product.Name, ImageKeys.AppIcon, stateViewModel, action, serviceProvider);
    }
}