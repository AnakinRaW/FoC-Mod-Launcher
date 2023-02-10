using System;
using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;

public abstract class CommandDefinition : ICommandDefinition
{
    public virtual ImageKey Image => default;

    public virtual string? Tooltip => null;

    public abstract string Text { get; }

    public abstract ICommand Command { get; }
}

public abstract class CommandDefinition<T> : CommandDefinition where T : ICommandHandler
{
    public sealed override ICommand Command { get; }

    protected CommandDefinition(IServiceProvider serviceProvider)
    {
        Command = serviceProvider.GetRequiredService<T>().Command;
    }
}