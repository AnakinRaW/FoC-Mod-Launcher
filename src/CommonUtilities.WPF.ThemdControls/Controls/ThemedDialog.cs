using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class ThemedDialog : ModalWindow
{
    static ThemedDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedDialog), new FrameworkPropertyMetadata(typeof(ThemedDialog)));
    }

    public ThemedDialog(IDialogViewModel viewModel) : base(viewModel)
    {
    }
}