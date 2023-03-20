using System;
using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Imaging;

namespace FocLauncher.ViewModels.Dialogs;

internal class UpdateRestartDialog : UpdateImageDialog, IUpdateRestartDialog
{
    internal const string RestartButtonIdentifier = "restart";
    internal const string NotNowButtonIdentifier = "not-now";

    private readonly bool _restartElevated;

    public override ImageKey Image => ImageKeys.Palpatine;

    public string Header { get; }

    public string Message { get; }

    private UpdateRestartDialog(string header, string message, bool restartElevated, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _restartElevated = restartElevated;
        Header = header;
        Message = message;

    }

    protected override IList<IButtonViewModel> CreateButtons(IDialogButtonFactory buttonFactory)
    {
        var buttons = new List<IButtonViewModel>
        {
            buttonFactory.CreateCustom(RestartButtonIdentifier,
                DialogButtonCommandsDefinitions.Create("Restart", _restartElevated ? ImageKeys.UACShield : default)),
            buttonFactory.CreateCustom(NotNowButtonIdentifier, DialogButtonCommandsDefinitions.Create("Not Now"), true)
        };
        return buttons;
    }

    public static IUpdateRestartDialog CreateFailedRestore(IServiceProvider serviceProvider)
    {
        return new UpdateRestartDialog(
            "Restoring from broken update failed.",
            "The application will be reset on next start.",
            false,
            serviceProvider);
    }

    public static IUpdateRestartDialog CreateRestart(IServiceProvider serviceProvider)
    {
        return new UpdateRestartDialog(
            "Update requires restart", 
            "The update will be completed after a restart of the application.", 
            false, 
            serviceProvider);
    }

    public static IUpdateRestartDialog CreateElevationRestart(IServiceProvider serviceProvider)
    {
        return new UpdateRestartDialog(
            "Update requires additional permissions", 
            "For a successful update, administrator rights are required, because some files or directories have write protection.", 
            true, 
            serviceProvider);
    }
}