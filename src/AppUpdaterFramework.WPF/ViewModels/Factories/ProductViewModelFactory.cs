using System;
using AnakinRaW.AppUpdaterFramework.Commands;
using AnakinRaW.AppUpdaterFramework.Imaging;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Updater;
using AnakinRaW.AppUpdaterFramework.ViewModels.ProductStates;
using AnakinRaW.AppUpdaterFramework.ViewModels.Progress;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.ViewModels.Factories;

internal class ProductViewModelFactory : IProductViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppDispatcher _dispatcher;
    private readonly IUpdateCommandsFactory _commandsFactory;

    public ProductViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _dispatcher = serviceProvider.GetRequiredService<IAppDispatcher>();
        _commandsFactory = serviceProvider.GetRequiredService<IUpdateCommandsFactory>();
    }

    public IProductViewModel Create(IInstalledProduct product, IUpdateCatalog? updateCatalog)
    {
        IProductStateViewModel stateViewModel;
        ICommandDefinition? action = null;
        if (updateCatalog is null || updateCatalog.Action == UpdateCatalogAction.None)
        {
            if (product.State == ProductState.RestartRequired)
                action = _commandsFactory.CreateRestart();

            if (product.State.HasFlag(ProductState.ElevationRequired))
                action = _commandsFactory.CreateElevate();

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

        return new ProductViewModel(product.Name, ImageKeys.StatusOK, stateViewModel, action, _serviceProvider);
    }

    public IProductViewModel Create(IUpdateSession updateSession)
    {
        Requires.NotNull(updateSession, nameof(updateSession));

        var cancelCommand = new CancelUpdateCommand(updateSession);
        var progressViewModel = CreateProgressViewModel(updateSession);
        var updatingViewModel = new UpdatingStateViewModel(progressViewModel, _serviceProvider);

        return new ProductViewModel(updateSession.Product.Name, ImageKeys.StatusOK, updatingViewModel, cancelCommand, _serviceProvider);
    }

    private IProgressViewModel CreateProgressViewModel(IUpdateSession updateSession)
    {
        return _dispatcher.Invoke(() =>
        {
            var downloadProgressBarViewModel = new DownloadingProgressBarViewModel(updateSession, _serviceProvider);
            var installProgressBarViewModel = new InstallingProgressBarViewModel(updateSession, _serviceProvider);
            return new ProgressViewModel(_serviceProvider, downloadProgressBarViewModel, installProgressBarViewModel);
        });
    }
}