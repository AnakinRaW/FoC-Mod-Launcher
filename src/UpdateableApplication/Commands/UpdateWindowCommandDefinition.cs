using System;
using System.Windows.Input;
using AnakinRaW.ApplicationBase.Commands.Handlers;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.ApplicationBase.Commands;

public class UpdateWindowCommandDefinition : ICommandDefinition
{ 
    public string Text => "Update FoC Launcher";
    public string? Tooltip => null;
    public ImageKey Image => ImageKeys.UpdateIcon;
    public ICommand Command { get; }

    public UpdateWindowCommandDefinition(IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IShowUpdateWindowCommandHandler>();
        Command = handler.Command;
    }
}