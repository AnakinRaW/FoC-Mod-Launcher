using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.FileLocking.Interaction;
using FocLauncher.ViewModels.Dialogs;

namespace FocLauncher.Update.ViewModels.Dialogs;

internal interface IKillProcessDialog : IImageDialogViewModel
{
    IFileInfo LockedFile { get; }

    IEnumerable<ILockingProcess> LockingProcesses { get; }
}