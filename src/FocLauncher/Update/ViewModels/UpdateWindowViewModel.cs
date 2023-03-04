using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using AnakinRaW.AppUpaterFramework.Services;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.AppUpdaterFramework.Updater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FocLauncher.Services;
using FocLauncher.Update.LauncherImplementations;
using FocLauncher.Utilities;
using FocLauncher.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskCanceledException = System.Threading.Tasks.TaskCanceledException;

namespace FocLauncher.Update.ViewModels;

internal partial class UpdateWindowViewModel : ModalWindowViewModel, IUpdateWindowViewModel
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

    private IUpdateSession? _currentUpdateSession;

    [ObservableProperty]
    private bool _isLoadingBranches = true;
    
    [ObservableProperty]
    private bool _isCheckingForUpdate;

    [ObservableProperty]
    private bool _isUpdating;

    public bool CanSwitchBranches => !IsLoadingBranches && !IsCheckingForUpdate && !IsUpdating;

    [ObservableProperty]
    private ProductBranch _currentBranch = null!;

    [ObservableProperty] private IProductViewModel _productViewModel = null!;

    public IUpdateInfoBarViewModel InfoBarViewModel { get; } 

    public ObservableCollection<ProductBranch> Branches { get; } = new();

    public UpdateWindowViewModel(IServiceProvider serviceProvider)
    {
        var s = serviceProvider.CreateScope();

        _serviceProvider = serviceProvider;
        Title = "Launcher Update";
        HasMaximizeButton = false;
        HasMinimizeButton = false;
        IsResizable = false;
        InfoBarViewModel = new UpdateInfoBarViewModel(serviceProvider);

        _updateService = serviceProvider.GetRequiredService<IUpdateService>();
        _logger = serviceProvider.GetService<LoggerFactory>()?.CreateLogger(GetType());
        _connectionManager = serviceProvider.GetRequiredService<IConnectionManager>();
        _productService = _serviceProvider.GetRequiredService<IProductService>();
        _productViewModelFactory = _serviceProvider.GetRequiredService<IProductViewModelFactory>();
        _semaphoreLock = new SemaphoreSlim(1);
        RegisterEvents();
    }

    public Task InitializeAsync()
    {
        return Task.Run(async () =>
        {
            await InfoBarViewModel.InitializeAsync();
            await LoadInstalledLauncherInformation(null);

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

    private async Task LoadInstalledLauncherInformation(IUpdateCatalog? updateCatalog)
    {
        await _semaphoreLock.WaitAsync();
        try
        {
            var launcher = _productService.GetCurrentInstance();
            AppDispatcher.Invoke(() => ProductViewModel =
                _productViewModelFactory.Create(launcher, updateCatalog));

            if (updateCatalog?.Action is UpdateCatalogAction.Install or UpdateCatalogAction.Uninstall)
            {
                _serviceProvider.GetRequiredService<IQueuedDialogService>()
                    .ShowDialog(new ErrorMessageDialogViewModel(
                        "Update Error", "Update information has invalid data:\r\n" +
                                        "The launcher cannot be uninstalled or re-installed through this dialog.", _serviceProvider)).Forget();
            }
        }
        finally
        {
            _semaphoreLock.Release();
        }
    }

    private async Task LoadUpdatingLauncherInformation(IUpdateSession updateSession)
    {
        await _semaphoreLock.WaitAsync();
        try
        {
            AppDispatcher.Invoke(() => ProductViewModel =
                _productViewModelFactory.Create(updateSession));
        }
        finally
        {
            _semaphoreLock.Release();
        }
    }

    private async Task<bool> LoadBranches()
    {
        var launcher = _productService.GetCurrentInstance();

        if (launcher.Branch is null)
            throw new InvalidOperationException("Current installation does not have a branch.");

        try
        {
            IsLoadingBranches = true;
            
            AppDispatcher.Invoke(() =>
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
                var stableBranch = branches.FirstOrDefault(b => b.Name == LauncherBranchManager.StableBranchName) ??
                                   throw new InvalidOperationException(
                                       "No stable branch found. There is something wrong the deployment. Please call the author.");
                currentBranch = branches.FirstOrDefault(b => b.Equals(launcher.Branch)) ?? stableBranch;
            }
            else
            {
                branches = new List<ProductBranch> { launcher.Branch };
                currentBranch = launcher.Branch;
            }

            AppDispatcher.Invoke(() =>
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
            InfoBarViewModel.Status = UpdateStatus.Failed;
            _logger?.LogError(e, e.Message);
            AppDispatcher.Invoke(Branches.Clear);
            throw;
        }
        finally
        {
            IsLoadingBranches = false;
        }
    }

    private async Task CheckForUpdate()
    {
        if (!_connectionManager.HasInternetConnection())
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
            InfoBarViewModel.Status = UpdateStatus.Failed;
            _logger?.LogError(e, e.Message);
            var evm = new ErrorMessageDialogViewModel("Checking for updates failed.", e.Message, _serviceProvider);
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
        LoadInstalledLauncherInformation(e).Forget();
    }

    private void OnUpdateStarted(object sender, IUpdateSession e)
    {
        IsUpdating = true;
        _currentUpdateSession = e;
        LoadUpdatingLauncherInformation(e).Forget();
    }

    private void OnUpdateCompleted(object sender, EventArgs e)
    {
        IsUpdating = false;
        _currentUpdateSession = null;
        LoadInstalledLauncherInformation(null).Forget();
    }

    private void RegisterEvents()
    {
        _updateService.CheckingForUpdatesCompleted += OnUpdateCheckCompleted;
        _updateService.UpdateStarted += OnUpdateStarted;
        _updateService.UpdateCompleted += OnUpdateCompleted;
    }

    private void UnregisterEvents()
    {
        _updateService.CheckingForUpdatesCompleted -= OnUpdateCheckCompleted;
        _updateService.UpdateStarted -= OnUpdateStarted;
        _updateService.UpdateCompleted -= OnUpdateCompleted;
    }
}