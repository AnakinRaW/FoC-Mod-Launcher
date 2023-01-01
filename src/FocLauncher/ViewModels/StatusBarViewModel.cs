using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using Sklavenwalker.CommonUtilities.Wpf.Input;

namespace FocLauncher.ViewModels;

public partial class StatusBarViewModel : ObservableObject, IStatusBarViewModel
{
    [ObservableProperty] private bool _isVisible = true;

    [ObservableProperty] private string _text = "123";

    public ICommand ClickCommand => new DelegateCommand(() => MessageBox.Show(""));

    [ObservableProperty] private Brush _background = Brushes.Transparent;


    public void SetBackground(ResourceKey resource)
    {
        Background = Application.Current.TryFindResource(resource) as Brush ?? Brushes.Transparent;
    }
}