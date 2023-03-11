using System;
using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Imaging;
using FocLauncher.Update.Commands.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher.Update.Commands;

internal class ElevateApplicationCommand : CommandDefinition
{
    public override string Text => "Restart as Admin";

    public override ICommand Command { get; }

    public override ImageKey Image => ImageKeys.UACShield;

    public ElevateApplicationCommand(IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IElevateApplicationCommandHandler>();
        Command = handler.Command;
    }
}