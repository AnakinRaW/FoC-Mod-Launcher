namespace AnakinRaW.AppUpdaterFramework.ViewModels.Progress;

internal interface IProgressViewModel
{
    bool ShowInstallProgressBar { get; }

    bool ShowDownloadThenInstallText { get; }

    IProgressBarViewModel DownloadProgressBarViewModel { get; }
    IProgressBarViewModel InstallProgressBarViewModel { get; }
}