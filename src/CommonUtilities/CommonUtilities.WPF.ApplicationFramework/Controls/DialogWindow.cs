using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Controls;

public class DialogWindow : AutoSizeModalWindow
{
    static DialogWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogWindow), new FrameworkPropertyMetadata(typeof(DialogWindow)));
    }

    public DialogWindow(IDialogViewModel viewModel) : base(viewModel)
    {
    }
}