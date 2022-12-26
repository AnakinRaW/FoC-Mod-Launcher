using CommunityToolkit.Mvvm.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

internal partial class InvisibleStatusBarViewModel : ObservableObject, IStatusBarViewModel
{
    [ObservableProperty]
    private bool _isVisible;
}