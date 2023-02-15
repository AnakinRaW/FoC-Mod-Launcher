using System;
using System.ComponentModel;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FocLauncher.Update.ViewModels;

internal partial class ProgressViewModel : ViewModelBase, IProgressViewModel
{
    [ObservableProperty] private bool _showInstallProgressBar;

    public IProgressBarViewModel DownloadProgressBarViewModel { get; }
    public IProgressBarViewModel InstallProgressBarViewModel { get; }

    [NotifyChangedIsLinkedToProperty(nameof(ShowInstallProgressBar))]
    public bool ShowDownloadThenInstallText => !ShowInstallProgressBar;

    public ProgressViewModel(IServiceProvider serviceProvider, IProgressBarViewModel downloadProgressBar, IProgressBarViewModel installProgressBar) : base(serviceProvider)
    {
        DownloadProgressBarViewModel = downloadProgressBar;
        InstallProgressBarViewModel = installProgressBar;
        PropertyChangedEventManager.AddHandler(InstallProgressBarViewModel, OnProgressChanged, nameof(IProgressBarViewModel.ProgressValue));
    }

    private void OnProgressChanged(object sender, PropertyChangedEventArgs e)
    {
        ShowInstallProgressBar = InstallProgressBarViewModel.ProgressValue > 0.0;
    }
}