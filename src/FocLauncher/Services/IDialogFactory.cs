using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace FocLauncher.Services;

internal interface IDialogFactory
{
    ModalWindow Create(IDialogViewModel viewModel);
}