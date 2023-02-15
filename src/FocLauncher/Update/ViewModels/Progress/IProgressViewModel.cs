namespace FocLauncher.Update.ViewModels;

internal interface IProgressViewModel
{
    bool ShowInstallProgressBar { get; }

    bool ShowDownloadThenInstallText { get; }

    IProgressBarViewModel DownloadProgressBarViewModel { get; }
    IProgressBarViewModel InstallProgressBarViewModel { get; }
}