using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

namespace FocLauncher.Controls;

public partial class ImageDialog
{
    public ImageDialog(IDialogViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}