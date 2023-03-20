using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Updater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Commands.Handlers;

internal class UpdateCommandHandler : AsyncCommandHandlerBase<IUpdateCatalog>, IUpdateCommandHandler
{
    private readonly ILogger? _logger;
    private readonly IUpdateService _updateService;

    private readonly IAppDispatcher _dispatcher;
    private readonly IUpdateResultHandler _resultHandler;



    private bool _isUpdateInProgress;

    public UpdateCommandHandler(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _resultHandler = serviceProvider.GetRequiredService<IUpdateResultHandler>();
        
        _dispatcher = serviceProvider.GetRequiredService<IAppDispatcher>();

        _updateService = serviceProvider.GetRequiredService<IUpdateService>();
        _updateService.UpdateStarted += OnUpdateStarted;
        _updateService.UpdateCompleted += OnUpdateCompleted;
    }

    private void OnUpdateCompleted(object sender, EventArgs e)
    {
        _isUpdateInProgress = false;
        _dispatcher.BeginInvoke(DispatcherPriority.Background, CommandManager.InvalidateRequerySuggested);
    }

    private void OnUpdateStarted(object sender, IUpdateSession e)
    {
        _isUpdateInProgress = true;
        _dispatcher.BeginInvoke(DispatcherPriority.Background, CommandManager.InvalidateRequerySuggested);
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

        await _resultHandler.Handle(updateResult);
    }

    public override bool CanHandle(IUpdateCatalog? parameter)
    {
        return !_isUpdateInProgress;
    }

    private Task<UpdateResult> UpdateAsync(IUpdateCatalog parameter)
    {
        return _updateService.Update(parameter);
    }
}