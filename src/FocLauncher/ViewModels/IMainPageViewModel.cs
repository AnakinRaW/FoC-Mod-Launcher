﻿using System.Collections.ObjectModel;
using PetroGlyph.Games.EawFoc;

namespace FocLauncher.ViewModels;

public interface IMainPageViewModel : ILoadingViewModel
{
    ObservableCollection<object> Games { get; }

    object? SelectedGameObject { get; set; }

    IGameArgumentsViewModel ArgumentsViewModel { get; }
}