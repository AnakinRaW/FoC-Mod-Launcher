using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

public class ButtonDefinition : ICommandBarItemDefinition, ICommandItemDefinition
{
    public ICommandDefinition CommandDefinition { get; }

    public string Text => CommandDefinition.Text;

    public string? ToolTop => CommandDefinition.Tooltip;

    internal ButtonDefinition(ICommandDefinition commandDefinition)
    {
        CommandDefinition = commandDefinition;
    }

    public static ButtonDefinition FromCommandDefinition(ICommandDefinition commandDefinition)
    {
        return new ButtonDefinition(commandDefinition);
    }
}