using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.ApplicationBase.ViewModels.Dialogs;
using AnakinRaW.AppUpdaterFramework.FileLocking.Interaction;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using Validation;

namespace AnakinRaW.ApplicationBase.Update.ApplicationImplementations;

public class ApplicationUpdateInteractionFactory : IUpdateDialogViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ApplicationUpdateInteractionFactory(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
    }

    public IDialogViewModel CreateErrorViewModel(string message)
    {
        return new UpdateErrorDialog(message, _serviceProvider);
    }

    public IDialogViewModel CreateRestartViewModel(RestartReason reason)
    {
        return reason switch
        {
            RestartReason.Elevation => UpdateRestartDialog.CreateElevationRestart(_serviceProvider),
            RestartReason.Update => UpdateRestartDialog.CreateRestart(_serviceProvider),
            RestartReason.FailedRestore => UpdateRestartDialog.CreateFailedRestore(_serviceProvider), 
            _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
        };
    }

    public IDialogViewModel CreateKillProcessesViewModel(IFileInfo file, IEnumerable<ILockingProcess> lockingProcesses)
    {
        return new KillProcessDialogViewModel(file, lockingProcesses, _serviceProvider);
    }
}