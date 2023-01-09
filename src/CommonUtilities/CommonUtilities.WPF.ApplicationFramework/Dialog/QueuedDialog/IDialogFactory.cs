using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Controls;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public interface IDialogFactory
{
    DialogWindow Create(IDialogViewModel viewModel);
}