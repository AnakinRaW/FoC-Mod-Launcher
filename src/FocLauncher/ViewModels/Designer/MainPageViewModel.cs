using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using PetroGlyph.Games.EawFoc;

namespace FocLauncher.ViewModels.Designer;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class MainPageViewModel : IMainPageViewModel
{
    private IPlayableObject? _selectedGameObject;
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsLoading { get; }
    public string? LoadingText { get; }

    public ObservableCollection<object> Games { get; }

    public object? SelectedGameObject { get; set; }


    public IGameArgumentsViewModel ArgumentsViewModel { get; }

    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }
}