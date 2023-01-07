using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using FocLauncher.Services;
using FocLauncher.Update.ProductMetadata;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;
using Sklavenwalker.ProductMetadata;
using Sklavenwalker.ProductMetadata.Services;
using Sklavenwalker.ProductUpdater.Services;

namespace FocLauncher.Update.ViewModels;

internal partial class UpdateWindowViewModel : ModalWindowViewModel, IUpdateWindowViewModel
{
    private static readonly ProductBranch LoadingProductBranch = new ("Loading...", null!, true);

    private readonly IServiceProvider _serviceProvider;

    private readonly CancellationTokenSource _updateWindowCancellationTokenSource = new();

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string? _loadingText;
    
    [ObservableProperty]
    private ProductBranch _currentBranch = null!;

    [ObservableProperty] private IInstalledProductViewModel _installedProductViewModel = null!;

    public ObservableCollection<ProductBranch> Branches { get; } = new();


    public UpdateWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Launcher Update";
        HasMaximizeButton = false;
        HasMinimizeButton = false;
        IsResizable = false;
    }

    public Task InitializeAsync()
    {
        return Task.Run(async () =>
        {
            var launcher = LoadLauncherInformationAsync();
            await LoadBranches(launcher);
            await CheckForUpdate();
            IsLoading = false;
        });
    }

    private async Task LoadBranches(IInstalledProduct launcher)
    {
        AppDispatcher.Invoke(() =>
        {
            var loadingBranch = LoadingProductBranch;
            Branches.Add(loadingBranch);
            CurrentBranch = loadingBranch;
        });

        var branchManager = _serviceProvider.GetRequiredService<IBranchManager>();

        var branches = (await branchManager.GetAvailableBranches()).ToList();

        var stableBranch = branches.FirstOrDefault(b => b.Name == LauncherBranchManager.StableBranchName) ??
                           throw new InvalidOperationException("No stable branch found. There is something wrong the deployment. Please call the author.");

        var currentBranch = branches.FirstOrDefault(b => b.Equals(launcher.Branch)) ?? stableBranch;

        AppDispatcher.Invoke(() =>
        {
            Branches.Clear();
            foreach (var branch in branches)
                Branches.Add(branch);
            CurrentBranch = currentBranch;
        });
    }


    private IInstalledProduct LoadLauncherInformationAsync()
    {
        var productService = _serviceProvider.GetRequiredService<IProductService>();
        var launcher = productService.GetCurrentInstance();
        AppDispatcher.Invoke(() =>
            InstalledProductViewModel = new InstalledProductViewModel(launcher.Name, default, _serviceProvider));
        return launcher;
    }

    private async Task CheckForUpdate()
    {
        using var searchUpdateCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_updateWindowCancellationTokenSource.Token);

        var updateService = _serviceProvider.GetRequiredService<IProductUpdateProviderService>();

        //await updateService.CheckForUpdates(null, CancellationToken.None);

    }

    public void OnClosing(CancelEventArgs e)
    {
        _updateWindowCancellationTokenSource.Cancel();
    }
}

public interface IUpdateInfoBarViewModel
{
    string InformationText { get; set; }

    bool IsCheckingForUpdates { get; set; }

    ICommandDefinition SearchForUpdateCommand { get; }

    bool ShouldShowSearchButton { get; }
}