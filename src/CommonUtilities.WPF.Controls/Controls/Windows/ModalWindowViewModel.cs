using CommunityToolkit.Mvvm.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public partial class ModalWindowViewModel : WindowViewModel, IModalWindowViewModel
{
    [ObservableProperty]
    private bool _hasDialogFrame;

    [ObservableProperty]
    private bool _isCloseButtonEnabled = true;
}