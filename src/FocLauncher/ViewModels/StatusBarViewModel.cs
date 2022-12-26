using CommunityToolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

namespace FocLauncher.ViewModels;

public partial class StatusBarViewModel : ObservableObject, IStatusBarViewModel
{
    [ObservableProperty]
    private bool _isVisible = true;
}