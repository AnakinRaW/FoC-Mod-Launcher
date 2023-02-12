using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace FocLauncher.Update.ViewModels;

internal class ProgressViewModel : ViewModelBase, IProgressViewModel
{
    public IProgressBarViewModel DownloadProgressBarViewModel { get; }
    public IProgressBarViewModel InstallProgressBarViewModel { get; }

    public ProgressViewModel(IServiceProvider serviceProvider, IProgressBarViewModel downloadProgressBar, IProgressBarViewModel installProgressBar) : base(serviceProvider)
    {
        DownloadProgressBarViewModel = downloadProgressBar;
        InstallProgressBarViewModel = installProgressBar;
    }
}