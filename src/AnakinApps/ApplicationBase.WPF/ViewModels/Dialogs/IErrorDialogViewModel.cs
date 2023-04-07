using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace AnakinRaW.ApplicationBase.ViewModels.Dialogs;

public interface IErrorMessageDialogViewModel : IViewModel, IImageDialogViewModel
{
    string Header { get; }

    string Message { get; }
}