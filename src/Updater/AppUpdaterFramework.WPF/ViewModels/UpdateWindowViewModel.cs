using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.AppUpdaterFramework.Updater;
using AnakinRaW.AppUpdaterFramework.Utilities;
using AnakinRaW.AppUpdaterFramework.ViewModels.Factories;
using AnakinRaW.CommonUtilities.Windows;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskCanceledException = System.Threading.Tasks.TaskCanceledException;

namespace AnakinRaW.AppUpdaterFramework.ViewModels;

public partial class UpdateWindowViewModel : ModalWindowViewModel, IUpdateWindowViewModel
{
    private static readonly ProductBranch LoadingProductBranch = new ("Loading...", null!, true);

    private readonly SemaphoreSlim _semaphoreLock;
    private readonly IServiceProvider _serviceProvider;
    private readonly CancellationTokenSource _updateWindowCancellationTokenSource = new();
    private readonly IUpdateService _updateService;
    private readonly ILogger? _logger;
    private readonly IConnectionManager _connectionManager;
    private readonly IProductService _productService;
    private readonly IProductViewModelFactory _productViewModelFactory;
    private readonly IAppDispatcher _dispatcher;
    private readonly IUpdateDialogViewModelFactory _dialogViewModelFactory;

    private IUpdateSession? _currentUpdateSession;
    private bool _requiresRestart;
    private bool _requiresElevation;

    [ObservableProperty]
    private ProductBranch _currentBranch = null!;

    [ObservableProperty]
    private IProductViewModel _productViewModel = null!;

    [ObservableProperty]
    private bool _isLoadingBranches = true;
    
    [ObservableProperty]
    private bool _isCheckingForUpdate;

    [ObservableProperty]
    private bool _isUpdating;

    private bool RequiresRestart

    {
        get => _requiresRestart;
        set
        {
            if (_requiresRestart == value)
                return;
            _requiresRestart = value;
            OnPropertyChanged(nameof(CanSwitchBranches));
        }
    }

    private bool RequiresElevation

    {
        get => _requiresElevation;
        set
        {
            if (_requiresElevation == value)
                return;
            _requiresElevation = value;
            OnPropertyChanged(nameof(CanSwitchBranches));
        }
    }

    public bool CanSwitchBranches => !IsLoadingBranches && !IsCheckingForUpdate && !IsUpdating && !RequiresRestart && !RequiresElevation;
    
    public IUpdateInfoBarViewModel InfoBarViewModel { get; } 

    public ObservableCollection<ProductBranch> Branches { get; } = new();

    public UpdateWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Application Update";
        HasMaximizeButton = false;
        HasMinimizeButton = false;
        IsResizable = false;
        InfoBarViewModel = new UpdateInfoBarViewModel(serviceProvider);

        _updateService = serviceProvider.GetRequiredService<IUpdateService>();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _connectionManager = serviceProvider.GetRequiredService<IConnectionManager>();
        _productService = _serviceProvider.GetRequiredService<IProductService>();
        _productViewModelFactory = _serviceProvider.GetRequiredService<IProductViewModelFactory>();
        _dispatcher = _serviceProvider.GetRequiredService<IAppDispatcher>();
        _dialogViewModelFactory = _serviceProvider.GetRequiredService<IUpdateDialogViewModelFactory>();
        _semaphoreLock = new SemaphoreSlim(1);
        RegisterEvents();
    }

    public Task InitializeAsync()
    {
        return Task.Run(async () =>
        {
            await InfoBarViewModel.InitializeAsync();
            await LoadInstalledProductInformation(null);

            var downloaded = await LoadBranches();
            if (downloaded)
                CheckForUpdate().Forget();
        });
    }

    public override void OnClosing(CancelEventArgs e)
    {
        if (e.Cancel)
            return;
        try
        {
            _updateWindowCancellationTokenSource.Cancel();
            _currentUpdateSession?.Cancel();
        }
        catch (TaskCanceledException)
        {
            // Ignore
        }
        UnregisterEvents();
        InfoBarViewModel.Dispose();
    }

    private async Task LoadInstalledProductInformation(IUpdateCatalog? updateCatalog)
    {
        await _semaphoreLock.WaitAsync();
        try
        {
            var product = _productService.GetCurrentInstance();

            if (product.State == ProductState.RestartRequired)
                RequiresRestart = true;

            if (product.State == ProductState.ElevationRequired)
                RequiresElevation = true;

            _dispatcher.Invoke(() => ProductViewModel =
                _productViewModelFactory.Create(product, updateCatalog));

            if (updateCatalog?.Action is UpdateCatalogAction.Install or UpdateCatalogAction.Uninstall)
            {
                var model = _dialogViewModelFactory.CreateErrorViewModel(
                    "Installing or uninstalling a product is currently not supported.");
                _serviceProvider.GetRequiredService<IQueuedDialogService>().ShowDialog(model).Forget();
            }
        }
        finally
        {
            _semaphoreLock.Release();
        }
    }

    private async Task LoadUpdatingProductInformation(IUpdateSession updateSession)
    {
        await _semaphoreLock.WaitAsync();
        try
        {
            _dispatcher.Invoke(() => ProductViewModel =
                _productViewModelFactory.Create(updateSession));
        }
        finally
        {
            _semaphoreLock.Release();
        }
    }

    private async Task<bool> LoadBranches()
    {
        var currentInstance = _productService.GetCurrentInstance();

        if (currentInstance.Branch is null)
            throw new InvalidOperationException("Current installation does not have a branch.");

        try
        {
            IsLoadingBranches = true;

            _dispatcher.Invoke(() =>
            {
                var loadingBranch = LoadingProductBranch;
                Branches.Add(loadingBranch);
                CurrentBranch = loadingBranch;
            });

            var branchManager = _serviceProvider.GetRequiredService<IBranchManager>();

            IList<ProductBranch> branches;
            ProductBranch currentBranch;

            var hasInternet =_connectionManager.HasInternetConnection();
            if (hasInternet)
            {
                branches = (await branchManager.GetAvailableBranches()).ToList();
                var stableBranch = branches.FirstOrDefault(b => b.Name == branchManager.StableBranchName) ??
                                   throw new InvalidOperationException(
                                       "No stable branch found. There is something wrong the deployment. Please call the author.");
                currentBranch = branches.FirstOrDefault(b => b.Equals(currentInstance.Branch)) ?? stableBranch;
            }
            else
            {
                branches = new List<ProductBranch> { currentInstance.Branch };
                currentBranch = currentInstance.Branch;
            }

            _dispatcher.Invoke(() =>
            {
                Branches.Clear();
                foreach (var branch in branches)
                    Branches.Add(branch);
                CurrentBranch = currentBranch;
            });
            return hasInternet;
        }
        catch (Exception e)
        {
            InfoBarViewModel.Status = UpdateStatus.FailedChecking;
            _logger?.LogError(e, e.Message);
            _dispatcher.Invoke(Branches.Clear);
            throw;
        }
        finally
        {
            IsLoadingBranches = false;
        }
    }

    private async Task CheckForUpdate()
    {
        if (RequiresRestart || RequiresElevation || !_connectionManager.HasInternetConnection())
            return;

        try
        {
            IsCheckingForUpdate = true;
            using var searchUpdateCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(_updateWindowCancellationTokenSource.Token);
            
            var updateRef = _serviceProvider.GetRequiredService<IProductService>().CreateProductReference(null, CurrentBranch);
            await _updateService.CheckForUpdates(updateRef, searchUpdateCancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            // Ignore because cancelling checking is only possible by closing the window.
            // No need to report to InfoBar in this case.
        }
        catch (Exception e)
        {
            InfoBarViewModel.Status = UpdateStatus.FailedChecking;
            _logger?.LogError(e, e.Message);

            var evm = _dialogViewModelFactory.CreateErrorViewModel(e.Message);
            await _serviceProvider.GetRequiredService<IQueuedDialogService>().ShowDialog(evm);
        }
        finally
        {
            IsCheckingForUpdate = false;
        }
    }

    partial void OnCurrentBranchChanged(ProductBranch value)
    {
        if (IsLoadingBranches)
            return;
        CheckForUpdate().Forget();
    }

    partial void OnIsLoadingBranchesChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSwitchBranches));
    }

    partial void OnIsCheckingForUpdateChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSwitchBranches));
    }

    private void OnUpdateCheckCompleted(object sender, IUpdateCatalog? e)
    {
        LoadInstalledProductInformation(e).Forget();
    }

    private void OnUpdateStarted(object sender, IUpdateSession e)
    {
        IsUpdating = true;
        _currentUpdateSession = e;
        LoadUpdatingProductInformation(e).Forget();
    }

    private void OnUpdateCompleted(object sender, EventArgs e)
    {
        IsUpdating = false;
        _currentUpdateSession = null;
        LoadInstalledProductInformation(null).Forget();
    }

    private void RegisterEvents()
    {
        _updateService.CheckingForUpdatesCompleted += OnUpdateCheckCompleted!;
        _updateService.UpdateStarted += OnUpdateStarted!;
        _updateService.UpdateCompleted += OnUpdateCompleted!;
    }

    private void UnregisterEvents()
    {
        _updateService.CheckingForUpdatesCompleted -= OnUpdateCheckCompleted!;
        _updateService.UpdateStarted -= OnUpdateStarted!;
        _updateService.UpdateCompleted -= OnUpdateCompleted!;
    }
}