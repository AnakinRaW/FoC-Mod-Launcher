using System;
using System.Threading.Tasks;
using System.Windows.Input;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.AppUpaterFramework.Updater;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FocLauncher.Update.Commands;

internal class UpdateCommand : CommandDefinition
{
    public IUpdateCatalog UpdateCatalog { get; }

    public override ImageKey Image => default;

    public override ICommand Command { get; }

    public override string Text { get; }

    public override string? Tooltip => null;

    public UpdateCommand(IUpdateCatalog updateCatalog, IServiceProvider serviceProvider, bool isRepair)
    {
        var handler = serviceProvider.GetRequiredService<IUpdateCommandHandler>();

        Command = new DelegateCommand(() => handler.Command.Execute(updateCatalog),
            () => handler.Command.CanExecute(updateCatalog));

        UpdateCatalog = updateCatalog;
        Text = isRepair ? "Repair" : "Update";
    }
}

internal interface IUpdateCommandHandler : ICommandHandler<IUpdateCatalog>
{
}

internal class UpdateCommandHandler : AsyncCommandHandlerBase<IUpdateCatalog>, IUpdateCommandHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;
    private readonly IUpdateProviderService _updateProviderService;

    public UpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _updateProviderService = serviceProvider.GetRequiredService<IUpdateProviderService>();
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

    private Task<object> UpdateAsync(IUpdateCatalog parameter)
    {
        return _updateProviderService.Update(parameter);
    }
}