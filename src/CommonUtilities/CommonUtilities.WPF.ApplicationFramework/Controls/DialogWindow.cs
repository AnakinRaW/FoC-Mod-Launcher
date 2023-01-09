using System.Windows;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Controls;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Controls;

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