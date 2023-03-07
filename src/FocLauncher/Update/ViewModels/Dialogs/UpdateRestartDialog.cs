using System;
using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Imaging;

namespace FocLauncher.Update.ViewModels.Dialogs;

internal class UpdateRestartDialog : UpdateImageDialog, IUpdateRestartDialog
{
    internal const string RestartButtonIdentifier = "restart";
    internal const string NotNowButtonIdentifier = "not-now";

    public override ImageKey Image => ImageKeys.Palpatine;

    public UpdateRestartDialog(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override IList<IButtonViewModel> CreateButtons(IDialogButtonFactory buttonFactory)
    {
        var buttons = new List<IButtonViewModel>()
        {
            buttonFactory.CreateCustom(RestartButtonIdentifier, DialogButtonCommandsDefinitions.Create("Restart")),
            buttonFactory.CreateCustom(NotNowButtonIdentifier, DialogButtonCommandsDefinitions.Create("Not Now"), true)
        };
        return buttons;
    }
}