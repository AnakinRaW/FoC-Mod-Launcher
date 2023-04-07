using System;
using System.Threading.Tasks;
using AnakinRaW.ApplicationBase.Imaging;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.ApplicationBase.ViewModels.Dialogs;

public class ErrorMessageDialogViewModel : DialogViewModel, IErrorMessageDialogViewModel
{
    public string Header { get; }

    public string Message { get; }

    public ImageKey Image => ImageKeys.Trooper;

    public ErrorMessageDialogViewModel(string header, string message, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Title = serviceProvider.GetRequiredService<IApplicationEnvironment>().ApplicationName;
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