using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Restart;
using AnakinRaW.AppUpdaterFramework.Updater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using FocLauncher.Services;
using FocLauncher.Update.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace FocLauncher.Update.Commands.Handlers;

internal class UpdateCommandHandler : AsyncCommandHandlerBase<IUpdateCatalog>, IUpdateCommandHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;
    private readonly IUpdateService _updateService;
    private bool _isUpdateInProgress;
    private readonly IQueuedDialogService _dialogService;

    public UpdateCommandHandler(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _dialogService = serviceProvider.GetRequiredService<IQueuedDialogService>();
        _updateService = serviceProvider.GetRequiredService<IUpdateService>();
        _updateService.UpdateStarted += OnUpdateStarted;
        _updateService.UpdateCompleted += OnUpdateCompleted;
    }

    private void OnUpdateCompleted(object sender, EventArgs e)
    {
        _isUpdateInProgress = false;
        AppDispatcher.BeginInvoke(DispatcherPriority.Background, CommandManager.InvalidateRequerySuggested);
    }

    private void OnUpdateStarted(object sender, IUpdateSession e)
    {
        _isUpdateInProgress = true;
        AppDispatcher.BeginInvoke(DispatcherPriority.Background, CommandManager.InvalidateRequerySuggested);
    }

    public override async Task HandleAsync(IUpdateCatalog parameter)
    {
        UpdateResult updateResult;
        try
        {
            updateResult = await UpdateAsync(parameter).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Unhandled exception {e.GetType()} encountered: {e.Message}");
            updateResult = new UpdateResult
            {
                Exception = e
            };
        }

        if (updateResult.RequiresElevation)
        {
            await HandleElevation();
            return;
        }

        if (updateResult.FailedRestore)
        {
            await HandleRestore();
            return;
        }

        if (updateResult.RestartType == RestartType.ApplicationRestart)
        {
            await HandleRestartRequired();
            return;
        }

        if (updateResult.Exception is null || updateResult.IsCanceled)
            return;
        
        await ShowError(updateResult);
    }

    public override bool CanHandle(IUpdateCatalog? parameter)
    {
        return !_isUpdateInProgress;
    }

    private async Task ShowError(UpdateResult updateResult)
    {
        var message = updateResult.Exception is AggregateException aggregateException
            ? aggregateException.InnerExceptions.First().Message
            : updateResult.Exception!.Message;
        await _dialogService.ShowDialog(new UpdateErrorDialog(message, _serviceProvider));
    }

    private async Task HandleRestartRequired()
    {
        var result = await _dialogService.ShowDialog(UpdateRestartDialog.CreateRestart(_serviceProvider));
        if (result != UpdateRestartDialog.RestartButtonIdentifier)
            return;
        new UpdateRestartCommand(_serviceProvider).Command.Execute(null);
    }

    private async Task HandleElevation()
    {
        var result = await _dialogService.ShowDialog(UpdateRestartDialog.CreateElevationRestart(_serviceProvider));
        if (result != UpdateRestartDialog.RestartButtonIdentifier)
            return;
        new ElevateApplicationCommand(_serviceProvider).Command.Execute(null);
    }

    private async Task HandleRestore()
    {

    }


    private Task<UpdateResult> UpdateAsync(IUpdateCatalog parameter)
    {
        return _updateService.Update(parameter);
    }
}