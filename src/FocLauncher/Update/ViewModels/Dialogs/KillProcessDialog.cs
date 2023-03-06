using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.FileLocking.Interaction;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Imaging;

namespace FocLauncher.Update.ViewModels.Dialogs;

internal class KillProcessDialog : UpdateImageDialog, IKillProcessDialog
{
    internal const string KillButtonIdentifier = "kill";

    public IFileInfo LockedFile { get; }

    public IEnumerable<ILockingProcess> LockingProcesses { get; }

    public override ImageKey Image => ImageKeys.Trooper;

    public KillProcessDialog(IFileInfo lockedFile, IEnumerable<ILockingProcess> lockingProcesses, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        LockedFile = lockedFile;
        LockingProcesses = lockingProcesses;
    }

    protected override IList<IButtonViewModel> CreateButtons(IDialogButtonFactory buttonFactory)
    {
        var buttons = new List<IButtonViewModel>
        {
            buttonFactory.CreateCustom(KillButtonIdentifier, DialogButtonCommandsDefinitions.Create("Kill"), false),
            buttonFactory.CreateRetry(false),
            buttonFactory.CreateCancel(true)
        };
        return buttons;
    }
}