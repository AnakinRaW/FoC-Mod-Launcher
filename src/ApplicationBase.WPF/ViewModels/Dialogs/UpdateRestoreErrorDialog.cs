using System;
using AnakinRaW.ApplicationBase.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace AnakinRaW.ApplicationBase.ViewModels.Dialogs;

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