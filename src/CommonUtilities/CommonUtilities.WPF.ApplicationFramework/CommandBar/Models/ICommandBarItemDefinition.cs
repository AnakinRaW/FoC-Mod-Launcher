namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

public interface ICommandBarItemDefinition
{
    string Text { get; }

    string? ToolTop { get; }
}