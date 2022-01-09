using System.Windows;
using FocLauncher.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace FocLauncher.Services;

internal class DialogFactory : IDialogFactory
{
    public DialogWindowBase Create(IDialogViewModel viewModel)
    {
        return Application.Current.Dispatcher.Invoke(() =>
        {
            var dialog = new ImageDialog();
            dialog.Initialize(viewModel);
            return dialog;
        });
    }
}