namespace FocLauncher.Update.ViewModels;

internal interface IProgressViewModel
{
    IProgressBarViewModel DownloadProgressBarViewModel { get; }
    IProgressBarViewModel InstallProgressBarViewModel { get; }
}