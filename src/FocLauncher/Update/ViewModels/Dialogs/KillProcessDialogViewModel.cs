using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.AppUpdaterFramework.FileLocking.Interaction;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Imaging;

namespace FocLauncher.Update.ViewModels.Dialogs;

internal class KillProcessDialogViewModel : UpdateImageDialog, IKillProcessDialogViewModel
{
    internal const string KillButtonIdentifier = "kill";

    public string Header => $"File '{LockedFile.Name}' is locked.";

    public IFileInfo LockedFile { get; }

    public IEnumerable<ILockingProcess> LockingProcesses { get; }

    public override ImageKey Image => ImageKeys.SwPulp;

    public KillProcessDialogViewModel(IFileInfo lockedFile, IEnumerable<ILockingProcess> lockingProcesses, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        LockedFile = lockedFile;
        LockingProcesses = lockingProcesses;
    }

    protected override IList<IButtonViewModel> CreateButtons(IDialogButtonFactory buttonFactory)
    {
        var killText = LockingProcesses.Count() == 1 ? "Kill Process" : "Kill Processes";
        var buttons = new List<IButtonViewModel>
        {
            buttonFactory.CreateCustom(KillButtonIdentifier, DialogButtonCommandsDefinitions.Create(killText)),
            buttonFactory.CreateRetry(false),
            buttonFactory.CreateCancel(true)
        };
        return buttons;
    }
}