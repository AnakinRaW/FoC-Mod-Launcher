namespace FocLauncher.ViewModels.Dialogs;

internal interface IUpdateRestartDialog : IImageDialogViewModel
{
    string Header { get; }

    string Message { get; }
}