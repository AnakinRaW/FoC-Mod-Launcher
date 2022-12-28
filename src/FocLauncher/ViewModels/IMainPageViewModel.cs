using System.Collections.ObjectModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace FocLauncher.ViewModels;

public interface IMainPageViewModel : ILoadingViewModel
{
    ObservableCollection<object> Games { get; }

    object? SelectedGameObject { get; set; }

    IGameArgumentsViewModel ArgumentsViewModel { get; }
}