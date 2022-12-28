using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace FocLauncher.ViewModels;

public partial class GameArgumentsViewModel : ViewModelBase, IGameArgumentsViewModel
{
    [ObservableProperty] private object? _currentGameObject;

    public GameArgumentsViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}