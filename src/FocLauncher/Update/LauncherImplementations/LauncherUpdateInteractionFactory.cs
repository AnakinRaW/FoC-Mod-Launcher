using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater;
using AnakinRaW.AppUpdaterFramework.FileLocking.Interaction;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using FocLauncher.Commands;
using FocLauncher.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher.Update.LauncherImplementations;

public class LauncherUpdateInteractionFactory : IUpdateCommandsFactory, IUpdateDialogViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LauncherUpdateInteractionFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ICommandDefinition CreateRestart()
    {
        var arguments = _serviceProvider.GetRequiredService<IExternalUpdaterService>().CreateUpdateArguments();
        return new UpdateRestartCommand(arguments, _serviceProvider);
    }

    public ICommandDefinition CreateElevate()
    {
        return new ElevateApplicationCommand(_serviceProvider);
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