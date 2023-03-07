using System;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Updater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using FocLauncher.Imaging;
using FocLauncher.Services;
using FocLauncher.Update.Commands;
using FocLauncher.Update.ViewModels.ProductStates;
using Validation;

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
            if (product.InstallState == ProductInstallState.RestartRequired)
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
            var isRepair = ProductReferenceEqualityComparer.VersionAware.Equals(product, updateCatalog.UpdateReference);
            action = new UpdateCommand(updateCatalog, _serviceProvider, isRepair);
        }

        return new ProductViewModel(product.Name, ImageKeys.AppIcon, stateViewModel, action, _serviceProvider);
    }

    public IProductViewModel Create(IUpdateSession updateSession)
    {
        Requires.NotNull(updateSession, nameof(updateSession));

        var cancelCommand = new CancelUpdateCommand(updateSession);
        var progressViewModel = CreateProgressViewModel(updateSession);
        var updatingViewModel = new UpdatingStateViewModel(progressViewModel, _serviceProvider);

        return new ProductViewModel(updateSession.Product.Name, ImageKeys.AppIcon, updatingViewModel, cancelCommand, _serviceProvider);
    }

    private IProgressViewModel CreateProgressViewModel(IUpdateSession updateSession)
    {
        return AppDispatcher.Invoke(() =>
        {
            var downloadProgressBarViewModel = new DownloadingProgressBarViewModel(updateSession, _serviceProvider);
            var installProgressBarViewModel = new InstallingProgressBarViewModel(updateSession, _serviceProvider);
            return new ProgressViewModel(_serviceProvider, downloadProgressBarViewModel, installProgressBarViewModel);
        });
    }
}