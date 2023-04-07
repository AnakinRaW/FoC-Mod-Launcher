using System;
using System.Windows.Input;
using AnakinRaW.ApplicationBase.Commands.Handlers;
using AnakinRaW.ApplicationBase.Imaging;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.ApplicationBase.Commands;

public class UpdateWindowCommandDefinition : ICommandDefinition
{ 
    public string Text { get; }
    public string? Tooltip => null;
    public ImageKey Image => ImageKeys.UpdateIcon;
    public ICommand Command { get; }

    public UpdateWindowCommandDefinition(IServiceProvider serviceProvider)
    {
        var handler = serviceProvider.GetRequiredService<IShowUpdateWindowCommandHandler>();
        var appName = serviceProvider.GetRequiredService<IApplicationEnvironment>().ApplicationName;
        Text = $"Update {appName}";
        Command = handler.Command;
    }
}