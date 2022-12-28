namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

public interface ILoadingViewModel : IViewModel
{
    bool IsLoading { get; }

    string? LoadingText { get; }
}