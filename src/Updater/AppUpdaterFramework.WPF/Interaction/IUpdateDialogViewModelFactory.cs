using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.FileLocking.Interaction;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

namespace AnakinRaW.AppUpdaterFramework.Interaction;

public interface IUpdateDialogViewModelFactory
{
    IDialogViewModel CreateErrorViewModel(string message);

    IDialogViewModel CreateRestartViewModel(RestartReason reason);

    IDialogViewModel CreateKillProcessesViewModel(IFileInfo file, IEnumerable<ILockingProcess> lockingProcesses);
}