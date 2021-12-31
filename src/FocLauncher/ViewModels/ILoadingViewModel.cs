namespace FocLauncher.ViewModels;

public interface ILoadingViewModel : ILauncherViewModel
{
    bool IsLoading { get; }

    string? LoadingText { get; }
}