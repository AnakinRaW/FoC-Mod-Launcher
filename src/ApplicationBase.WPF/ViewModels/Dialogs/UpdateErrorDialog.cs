using System;
using AnakinRaW.ApplicationBase.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace AnakinRaW.ApplicationBase.ViewModels.Dialogs;

internal class UpdateErrorDialog : UpdateImageDialog, IUpdateErrorDialog
{
    public override ImageKey Image => ImageKeys.Vader;

    public string Header => "Error while updating!";

    public string Message { get; }

    public UpdateErrorDialog(string message, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Message = message;
    }
}