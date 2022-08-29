using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FocLauncher.ViewModels;

public partial class GameArgumentsViewModel : ViewModelBase, IGameArgumentsViewModel
{
    [ObservableProperty] private object? _currentGameObject;

    public GameArgumentsViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}