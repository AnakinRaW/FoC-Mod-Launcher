using System;
using System.Windows.Input;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Input;
using FocLauncher.Update.Commands.Handlers;
using Microsoft.Extensions.DependencyInjection;

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