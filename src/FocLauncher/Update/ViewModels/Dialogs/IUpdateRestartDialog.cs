using FocLauncher.ViewModels.Dialogs;

namespace FocLauncher.Update.ViewModels.Dialogs;

internal interface IUpdateRestartDialog : IImageDialogViewModel
{
    string Header { get; }

    string Message { get; }
}