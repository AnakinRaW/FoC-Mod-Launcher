using System;
using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Update.Commands.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher.Update.Commands;

internal class UpdateRestartCommand : CommandDefinition
{
    public override ImageKey Image => default;
    public override string Text => "Restart";
    public override ICommand Command { get; }
    public override string? Tooltip => null;

    public UpdateRestartCommand(IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IUpdateRestartCommandHandler>();
        Command = handler.Command;
    }
}