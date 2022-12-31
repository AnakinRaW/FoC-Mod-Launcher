using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class AutoSizeModalWindow : ModalWindow
{
    static AutoSizeModalWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoSizeModalWindow), new FrameworkPropertyMetadata(typeof(AutoSizeModalWindow)));
    }

    public AutoSizeModalWindow(IModalWindowViewModel viewModel) : base(viewModel)
    {
    }
}