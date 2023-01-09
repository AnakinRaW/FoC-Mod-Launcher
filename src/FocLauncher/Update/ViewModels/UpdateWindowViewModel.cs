using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using AnakinRaW.CommonUtilities.Wpf.Controls;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductMetadata.Services;
using AnakinRaW.ProductUpdater.Catalog;
using AnakinRaW.ProductUpdater.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using FocLauncher.Imaging;
using FocLauncher.Services;
using FocLauncher.Update.ProductMetadata;
using FocLauncher.Utilities;
using Microsoft.Extensions.DependencyInjection;
using TaskCanceledException = System.Threading.Tasks.TaskCanceledException;

namespace FocLauncher.Update.ViewModels;

internal partial class UpdateWindowViewModel : ModalWindowViewModel, IUpdateWindowViewModel
{
    private static readonly ProductBranch LoadingProductBranch = new ("Loading...", null!, true);

    private readonly IServiceProvider _serviceProvider;

    private readonly CancellationTokenSource _updateWindowCancellationTokenSource = new();

    private readonly IProductUpdateProviderService _updateService;

    [ObservableProperty]
    private bool _isLoadingBranches = true;

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

        _updateService = _serviceProvider.GetRequiredService<IProductUpdateProviderService>();
        RegisterEvents();
    }

    public Task InitializeAsync()
    {
        return Task.Run(async () =>
        {
            await InfoBarViewModel.InitializeAsync();
            var launcher = LoadLauncherInformationAsync(null);
            await LoadBranches(launcher);
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

    private IInstalledProduct LoadLauncherInformationAsync(IUpdateCatalog? updateCatalog)
    {
        var productService = _serviceProvider.GetRequiredService<IProductService>();
        var launcher = productService.GetCurrentInstance();
        AppDispatcher.Invoke(() =>
            InstalledProductViewModel = new InstalledProductViewModel(launcher.Name, ImageKeys.AppIcon, _serviceProvider));
        return launcher;
    }


    private async Task LoadBranches(IInstalledProduct launcher)
    {
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

            var branches = (await branchManager.GetAvailableBranches()).ToList();

            var stableBranch = branches.FirstOrDefault(b => b.Name == LauncherBranchManager.StableBranchName) ??
                               throw new InvalidOperationException(
                                   "No stable branch found. There is something wrong the deployment. Please call the author.");

            var currentBranch = branches.FirstOrDefault(b => b.Equals(launcher.Branch)) ?? stableBranch;

            AppDispatcher.Invoke(() =>
            {
                Branches.Clear();
                foreach (var branch in branches)
                    Branches.Add(branch);
                CurrentBranch = currentBranch;
            });

            IsLoadingBranches = false;
        }
        catch (Exception)
        {
            InfoBarViewModel.Status = UpdateStatus.Failed;
            AppDispatcher.Invoke(Branches.Clear);
            throw;
        }
    }

    private async Task CheckForUpdate()
    {
        try
        {
            using var searchUpdateCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(_updateWindowCancellationTokenSource.Token);
                
            await _updateService.CheckForUpdates(null, searchUpdateCancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            // Ignore because cancelling checking is only possible by closing the window.
            // No need to report to InfoBar in this case.
        }
        catch (Exception e)
        {
            InfoBarViewModel.Status = UpdateStatus.Failed;
            // TODO: Error Dialog
        }
    }

    private void OnUpdateCheckCompleted(object sender, IUpdateCatalog? e)
    {
        LoadLauncherInformationAsync(e);
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