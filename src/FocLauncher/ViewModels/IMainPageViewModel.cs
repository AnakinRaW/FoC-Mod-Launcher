using System.Collections.ObjectModel;
using PetroGlyph.Games.EawFoc;

namespace FocLauncher.ViewModels;

public interface IMainPageViewModel : ILoadingViewModel
{
    ObservableCollection<IPlayableObject> Games { get; }

    IPlayableObject? SelectedGameObject { get; set; }

    IGameArgumentsViewModel ArgumentsViewModel { get; }
}