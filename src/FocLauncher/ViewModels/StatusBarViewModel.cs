using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using FocLauncher.Commands;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;
using Validation;

namespace FocLauncher.ViewModels;

public partial class StatusBarViewModel : ObservableObject, ILauncherStatusBarViewModel
{
    [ObservableProperty] private bool _isVisible = true;

    [ObservableProperty] private string _installedGameType = "Test";

    public ICommandDefinition AboutCommand { get; }
    public ICommandDefinition SettingsCommand => null;
    public ICommandDefinition UpdateCommand => null;

    [ObservableProperty] private Brush _background = Brushes.Transparent;

    public StatusBarViewModel(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        AboutCommand = new AboutDialogCommandDefinition(serviceProvider);
    }


    public void SetBackground(ResourceKey resource)
    {
        Background = Application.Current.TryFindResource(resource) as Brush ?? Brushes.Transparent;
    }
}