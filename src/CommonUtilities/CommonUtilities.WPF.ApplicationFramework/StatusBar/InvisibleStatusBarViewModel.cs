using CommunityToolkit.Mvvm.ComponentModel;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

internal partial class InvisibleStatusBarViewModel : ObservableObject, IStatusBarViewModel
{
    [ObservableProperty]
    private bool _isVisible;
}