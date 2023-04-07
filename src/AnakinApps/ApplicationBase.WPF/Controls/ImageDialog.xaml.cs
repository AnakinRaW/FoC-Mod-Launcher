using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

namespace AnakinRaW.ApplicationBase.Controls;

public partial class ImageDialog
{
    public ImageDialog(IDialogViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}