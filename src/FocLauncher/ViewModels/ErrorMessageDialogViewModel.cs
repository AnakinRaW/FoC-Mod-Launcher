using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FocLauncher.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.Services;

namespace FocLauncher.ViewModels;

public class ErrorMessageDialogViewModel : DialogViewModel, IErrorMessageDialogViewModel
{
    private readonly IServiceProvider _serviceProvider;

    public string Header { get; }

    public string Message { get; }

    public ImageMoniker Image => Monikers.Trooper;

    public ErrorMessageDialogViewModel(string header, string message, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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

    protected override IList<IButtonViewModel> CreateButtons()
    {
        var buttonFactory = _serviceProvider.GetRequiredService<IDialogButtonFactory>();
        var okButton = buttonFactory.CreateOk(true);
        var buttons = new List<IButtonViewModel> { okButton };
        return buttons;
    }
}