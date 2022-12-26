using CommunityToolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.Controls.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public partial class ModalWindowViewModel : WindowViewModel, IModalWindowViewModel
{
    [ObservableProperty]
    private bool _hasDialogFrame;

    [ObservableProperty]
    private bool _isCloseButtonEnabled = true;
}