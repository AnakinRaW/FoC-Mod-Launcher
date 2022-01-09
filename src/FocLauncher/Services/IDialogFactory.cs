using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace FocLauncher.Services;

internal interface IDialogFactory
{
    DialogWindowBase Create(IDialogViewModel viewModel);
}