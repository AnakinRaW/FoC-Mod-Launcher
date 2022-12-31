using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using Sklavenwalker.CommonUtilities.Wpf.Input;

namespace FocLauncher.ViewModels;

public partial class StatusBarViewModel : ObservableObject, IStatusBarViewModel
{
    [ObservableProperty] private bool _isVisible = true;

    [ObservableProperty] private string _text = "123";

    public ICommand ClickCommand => new DelegateCommand(() => MessageBox.Show(""));
}