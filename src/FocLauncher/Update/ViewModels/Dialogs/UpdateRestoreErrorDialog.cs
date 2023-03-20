using System;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Imaging;

namespace FocLauncher.Update.ViewModels.Dialogs;

internal class UpdateRestoreErrorDialog : UpdateImageDialog, IUpdateRestoreErrorDialog
{
    public override ImageKey Image => ImageKeys.Vader;
    public string Header => "Error while updating!";

    public string Message => "While restoring a failed update a critical error occurred.\r\n" +
                             "Restart the application to reset the application.";

    public UpdateRestoreErrorDialog(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}