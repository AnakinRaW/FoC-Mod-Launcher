using System;
using System.Linq;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.AppUpdaterFramework.Restart;
using AnakinRaW.AppUpdaterFramework.Updater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using FocLauncher.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher.Update.LauncherImplementations;

internal class LauncherUpdateResultHandler : IUpdateResultHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IQueuedDialogService _dialogService;
    private readonly IUpdateDialogViewModelFactory _dialogViewModelFactory;
    private readonly IApplicationUpdaterRegistry _updaterRegistry;

    public LauncherUpdateResultHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _dialogService = serviceProvider.GetRequiredService<IQueuedDialogService>();
        _dialogViewModelFactory = serviceProvider.GetRequiredService<IUpdateDialogViewModelFactory>();
        _updaterRegistry = serviceProvider.GetRequiredService<IApplicationUpdaterRegistry>();
    }

    public async Task Handle(UpdateResult result)
    {
        if (result.RestartType == RestartType.ApplicationElevation)
        {
            await HandleElevation();
            return;
        }

        if (result.FailedRestore)
        {
            await HandleRestore();
            return;
        }

        if (result.RestartType == RestartType.ApplicationRestart)
        {
            await HandleRestartRequired();
            return;
        }

        if (result.Exception is null || result.IsCanceled)
            return;

        await ShowError(result);
    }

    private async Task ShowError(UpdateResult updateResult)
    {
        var message = updateResult.Exception is AggregateException aggregateException
            ? aggregateException.InnerExceptions.First().Message
            : updateResult.Exception!.Message;
        var viewModel = _dialogViewModelFactory.CreateErrorViewModel(message);
        await _dialogService.ShowDialog(viewModel);
    }

    private async Task HandleRestartRequired()
    {
        var updateArguments = _serviceProvider.GetRequiredService<IExternalUpdaterService>().CreateUpdateArguments();
        var updater = _serviceProvider.GetRequiredService<IExternalUpdaterService>().GetExternalUpdater();

        _updaterRegistry.ScheduleUpdate(updater, updateArguments);

        var viewModel = _dialogViewModelFactory.CreateRestartViewModel(RestartReason.Update);
        var result = await _dialogService.ShowDialog(viewModel);
        if (result == UpdateDialogButtonIdentifiers.RestartButtonIdentifier)
            new UpdateRestartCommand(updateArguments, _serviceProvider).Command.Execute(null);
    }

    private async Task HandleElevation()
    {
        var viewModel = _dialogViewModelFactory.CreateRestartViewModel(RestartReason.Elevation);
        var result = await _dialogService.ShowDialog(viewModel);
        if (result != UpdateDialogButtonIdentifiers.RestartButtonIdentifier)
            return;
        new ElevateApplicationCommand(_serviceProvider).Command.Execute(null);
    }

    private async Task HandleRestore()
    {
        _updaterRegistry.ScheduleRestore();
        var viewModel = _dialogViewModelFactory.CreateRestartViewModel(RestartReason.FailedRestore);
        await _dialogService.ShowDialog(viewModel);
    }
}