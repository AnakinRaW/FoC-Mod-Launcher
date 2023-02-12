using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.AppUpaterFramework.Updater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using FocLauncher.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FocLauncher.Update.Commands;

internal class UpdateCommandHandler : AsyncCommandHandlerBase<IUpdateCatalog>, IUpdateCommandHandler
{
    private readonly ILogger? _logger;
    private readonly IUpdateService _updateService;
    private bool _isUpdateInProgress;

    public UpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
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
        try
        {
            var result = await UpdateAsync(parameter).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Unhandled exception {e.GetType()} encountered: {e.Message}");
            // TODO: Handle error state
        }
    }

    public override bool CanHandle(IUpdateCatalog? parameter)
    {
        return !_isUpdateInProgress;
    }

    private Task<object> UpdateAsync(IUpdateCatalog parameter)
    {
        return _updateService.Update(parameter);
    }
}