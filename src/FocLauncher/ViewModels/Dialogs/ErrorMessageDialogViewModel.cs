using System;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Imaging;

namespace FocLauncher.ViewModels.Dialogs;

public class ErrorMessageDialogViewModel : DialogViewModel, IErrorMessageDialogViewModel
{
    public string Header { get; }

    public string Message { get; }

    public ImageKey Image => ImageKeys.Trooper;

    public ErrorMessageDialogViewModel(string header, string message, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Title = LauncherEnvironment.LauncherProgramName;
        Header = header;
        Message = message;
        IsResizable = false;
        HasMaximizeButton = false;
        HasMinimizeButton = false;
        HasDialogFrame = true;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}