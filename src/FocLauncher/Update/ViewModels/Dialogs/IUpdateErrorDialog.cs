using FocLauncher.ViewModels.Dialogs;

namespace FocLauncher.Update.ViewModels.Dialogs;

public interface IUpdateErrorDialog : IImageDialogViewModel
{
    public string Header { get; }

    public string Message { get; }
}