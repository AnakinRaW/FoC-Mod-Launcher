using System;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Imaging;

namespace FocLauncher.Update.ViewModels.Dialogs;

internal class UpdateErrorDialog : UpdateImageDialog, IUpdateErrorDialog
{
    public override ImageKey Image => ImageKeys.Vader;

    public string Header => "Error while updating";

    public string Message { get; }

    public UpdateErrorDialog(string message, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Message = message;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}