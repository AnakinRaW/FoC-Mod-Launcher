using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.FileLocking.Interaction;
using FocLauncher.ViewModels.Dialogs;

namespace FocLauncher.Update.ViewModels.Dialogs;

internal interface IKillProcessDialogViewModel : IImageDialogViewModel
{
    string Header { get; }

    IFileInfo LockedFile { get; }

    IEnumerable<ILockingProcess> LockingProcesses { get; }
}