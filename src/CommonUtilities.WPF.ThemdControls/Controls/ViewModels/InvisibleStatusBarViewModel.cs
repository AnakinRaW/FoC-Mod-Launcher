using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

internal partial class InvisibleStatusBarViewModel : ObservableObject, IStatusBarViewModel
{
    [ObservableProperty]
    private bool _isVisible;
}