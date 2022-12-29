using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FocLauncher.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.ViewModels;

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

    protected override IList<IButtonViewModel> CreateButtons(IDialogButtonFactory buttonFactory)
    {
        var okButton = buttonFactory.CreateOk(true);
        var buttons = new List<IButtonViewModel> { okButton };
        return buttons;
    }
}