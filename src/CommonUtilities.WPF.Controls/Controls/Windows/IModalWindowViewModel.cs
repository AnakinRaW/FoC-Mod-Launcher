namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface IModalWindowViewModel : IWindowViewModel
{
    bool HasDialogFrame { get; set; }

    bool IsCloseButtonEnabled { get; set; }
}