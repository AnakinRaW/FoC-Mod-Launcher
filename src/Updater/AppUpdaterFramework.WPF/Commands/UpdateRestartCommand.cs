using System;
using System.Windows.Input;
using AnakinRaW.AppUpdaterFramework.Commands.Handlers;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Input;
using AnakinRaW.ExternalUpdater.CLI.Arguments;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.AppUpdaterFramework.Commands;

internal class UpdateRestartCommand : CommandDefinition
{
    public override ImageKey Image => default;
    public override string Text => "Restart";
    public override ICommand Command { get; }
    public override string? Tooltip => null;

    public UpdateRestartCommand(ExternalUpdaterArguments updaterArguments, IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IUpdateRestartCommandHandler>();
        Command = new DelegateCommand(() => handler.Command.Execute(updaterArguments),
            () => handler.Command.CanExecute(updaterArguments));
    }
}