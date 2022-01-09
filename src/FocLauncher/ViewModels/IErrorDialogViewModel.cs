namespace FocLauncher.ViewModels;

public interface IErrorMessageDialogViewModel : ILauncherViewModel, IImageDialogViewModel
{
    string Header { get; }

    string Message { get; }
}