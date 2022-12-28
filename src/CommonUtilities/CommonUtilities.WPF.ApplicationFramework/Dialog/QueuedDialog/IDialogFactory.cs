using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public interface IDialogFactory
{
    DialogWindow Create(IDialogViewModel viewModel);
}