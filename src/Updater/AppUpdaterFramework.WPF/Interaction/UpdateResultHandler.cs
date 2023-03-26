using System;
using System.Linq;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Commands.Handlers;
using AnakinRaW.AppUpdaterFramework.Configuration;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;
using AnakinRaW.AppUpdaterFramework.Restart;
using AnakinRaW.AppUpdaterFramework.Updater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.AppUpdaterFramework.Interaction;

internal class UpdateResultHandler : IUpdateResultHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IQueuedDialogService _dialogService;
    private readonly IUpdateDialogViewModelFactory _dialogViewModelFactory;
    private readonly IApplicationUpdaterRegistry _updaterRegistry;
    private readonly IUpdateConfiguration _updateConfiguration;
    private readonly IUpdateRestartCommandHandler _restartHandler;

    public UpdateResultHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _dialogService = serviceProvider.GetRequiredService<IQueuedDialogService>();
        _dialogViewModelFactory = serviceProvider.GetRequiredService<IUpdateDialogViewModelFactory>();
        _updaterRegistry = serviceProvider.GetRequiredService<IApplicationUpdaterRegistry>();
        _restartHandler = serviceProvider.GetRequiredService<IUpdateRestartCommandHandler>();
        _updateConfiguration = serviceProvider.GetRequiredService<IUpdateConfigurationProvider>().GetConfiguration();
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
            await HandleFailedRestore();
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
        if (!_updateConfiguration.SupportsRestart)
            return;

        var updateArguments = _serviceProvider.GetRequiredService<IExternalUpdaterService>().CreateUpdateArguments();
        var updater = _serviceProvider.GetRequiredService<IExternalUpdaterService>().GetExternalUpdater();

        _updaterRegistry.ScheduleUpdate(updater, updateArguments);

        var viewModel = _dialogViewModelFactory.CreateRestartViewModel(RestartReason.Update);
        var result = await _dialogService.ShowDialog(viewModel);
        if (result != UpdateDialogButtonIdentifiers.RestartButtonIdentifier)
            return;

        await _restartHandler.HandleAsync(updateArguments);
    }

    private async Task HandleElevation()
    {
        if (!_updateConfiguration.SupportsRestart)
            return;

        var viewModel = _dialogViewModelFactory.CreateRestartViewModel(RestartReason.Elevation);
        var result = await _dialogService.ShowDialog(viewModel);
        if (result != UpdateDialogButtonIdentifiers.RestartButtonIdentifier)
            return;

        var restartArgs = _serviceProvider.GetRequiredService<IExternalUpdaterService>().CreateRestartArguments(true);
        await _restartHandler.HandleAsync(restartArgs);
    }

    private async Task HandleFailedRestore()
    {
        _updaterRegistry.ScheduleReset();

        if (!_updateConfiguration.SupportsRestart)
            return;

        var viewModel = _dialogViewModelFactory.CreateRestartViewModel(RestartReason.FailedRestore);
        var result = await _dialogService.ShowDialog(viewModel);
        if (result != UpdateDialogButtonIdentifiers.RestartButtonIdentifier)
            return;

        var restartArgs = _serviceProvider.GetRequiredService<IExternalUpdaterService>().CreateRestartArguments();
        await _restartHandler.HandleAsync(restartArgs);
    }
}