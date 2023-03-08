using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.FileLocking.Interaction;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using FocLauncher.Update.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace FocLauncher.Update.LauncherImplementations;

public class LauncherInteractionHandler : IInteractionHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IQueuedDialogService _dialogService;

    public LauncherInteractionHandler(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        _dialogService = serviceProvider.GetRequiredService<IQueuedDialogService>();
    }

    public LockedFileHandlerInteractionResult HandleLockedFile(IFileInfo file, IEnumerable<ILockingProcess> lockingProcesses)
    {
        var result = ShowDialog(new KillProcessDialogViewModel(file, lockingProcesses, _serviceProvider));
        return result switch
        {
            DefaultDialogButtonIdentifiers.Retry => LockedFileHandlerInteractionResult.Retry,
            KillProcessDialogViewModel.KillButtonIdentifier => LockedFileHandlerInteractionResult.Kill,
            _ => LockedFileHandlerInteractionResult.Cancel
        };
    }

    public void HandleError(string message)
    {
        ShowDialog(new UpdateErrorDialog(message, _serviceProvider));
    }

    private string? ShowDialog(IDialogViewModel viewModel)
    {
        var task = Task.Run(async () => await _dialogService.ShowDialog(viewModel));
        task.Wait();
        return task.Result;
    }
}