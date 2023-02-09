using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.AppUpaterFramework.Product;
using AnakinRaW.AppUpaterFramework.Services;
using AnakinRaW.AppUpaterFramework.Updater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FocLauncher.Services;
using FocLauncher.Update.ProductMetadata;
using FocLauncher.Utilities;
using FocLauncher.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskCanceledException = System.Threading.Tasks.TaskCanceledException;

namespace FocLauncher.Update.ViewModels;

internal partial class UpdateWindowViewModel : ModalWindowViewModel, IUpdateWindowViewModel
{
    private static readonly ProductBranch LoadingProductBranch = new ("Loading...", null!, true);
    
    private readonly IServiceProvider _serviceProvider;
    private readonly CancellationTokenSource _updateWindowCancellationTokenSource = new();
    private readonly IUpdateProviderService _updateService;
    private readonly ILogger? _logger;
    private readonly IConnectionManager _connectionManager;
    private readonly IProductService _productService;
    private readonly IInstalledProductViewModelFactory _installedProductViewModelFactory;

    [ObservableProperty]
    private bool _isLoadingBranches = true;
    
    [ObservableProperty]
    private bool _isCheckingForUpdate;
    
    public bool CanSwitchBranches => !IsLoadingBranches && !IsCheckingForUpdate;

    [ObservableProperty]
    private ProductBranch _currentBranch = null!;

    [ObservableProperty] private IInstalledProductViewModel _installedProductViewModel = null!;

    public IUpdateInfoBarViewModel InfoBarViewModel { get; } 

    public ObservableCollection<ProductBranch> Branches { get; } = new();

    public UpdateWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Launcher Update";
        HasMaximizeButton = false;
        HasMinimizeButton = false;
        IsResizable = false;
        InfoBarViewModel = new UpdateInfoBarViewModel(serviceProvider);

        _updateService = serviceProvider.GetRequiredService<IUpdateProviderService>();
        _logger = serviceProvider.GetService<LoggerFactory>()?.CreateLogger(GetType());
        _connectionManager = serviceProvider.GetRequiredService<IConnectionManager>();
        _productService = _serviceProvider.GetRequiredService<IProductService>();
        _installedProductViewModelFactory = _serviceProvider.GetRequiredService<IInstalledProductViewModelFactory>();
        RegisterEvents();
    }

    public Task InitializeAsync()
    {
        return Task.Run(async () =>
        {
            await InfoBarViewModel.InitializeAsync();
            LoadLauncherInformation(null);

            await ExtractAssemblies();

            var downloaded = await LoadBranches();
            if (downloaded)
                CheckForUpdate().Forget();
        });
    }

    public override void OnClosing(CancelEventArgs e)
    {
        if (e.Cancel)
            return;
        _updateWindowCancellationTokenSource.Cancel();
        UnregisterEvents();
        InfoBarViewModel.Dispose();
    }

    private void LoadLauncherInformation(IUpdateCatalog? updateCatalog)
    {
       var launcher = _productService.GetCurrentInstance();

       AppDispatcher.Invoke(() => InstalledProductViewModel =
           _installedProductViewModelFactory.Create(launcher, updateCatalog, _serviceProvider));
       
       if (updateCatalog?.Action is UpdateCatalogAction.Install or UpdateCatalogAction.Uninstall)
       {
           _serviceProvider.GetRequiredService<IQueuedDialogService>()
               .ShowDialog(new ErrorMessageDialogViewModel(
                   "Update Error", "Update information has invalid data:\r\n" + 
                                   "The launcher cannot be uninstalled or re-installed through this dialog.", _serviceProvider)).Forget();
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

    private async Task ExtractAssemblies()
    {
        var env = _serviceProvider.GetRequiredService<ILauncherEnvironment>();
        await _serviceProvider.GetRequiredService<ICosturaAssemblyExtractor>()
            .ExtractAssemblyAsync(LauncherConstants.AppUpdaterAssemblyName, env.ApplicationLocalPath);
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
        LoadLauncherInformation(e);
    }

    private void RegisterEvents()
    {
        _updateService.CheckingForUpdatesCompleted += OnUpdateCheckCompleted;
    }

    private void UnregisterEvents()
    {
        _updateService.CheckingForUpdatesCompleted -= OnUpdateCheckCompleted;
    }
}