using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Builder;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

public class MenuDefinition : IMenuDefinition
{
    public IReadOnlyList<ICommandBarGroup> Groups { get; }

    public string Text { get; }

    public string? ToolTop { get; }

    public bool IsEnabled { get; }

    internal MenuDefinition(string text, bool isEnabled, string? toolTop, IReadOnlyList<ICommandBarGroup> groups)
    {
        Groups = groups;
        Text = text;
        IsEnabled = isEnabled;
        ToolTop = toolTop;
    }

    public static ICommandBarBuilderStartContext<IMenuDefinition> Builder(string text, bool enable = true, string? tooltip = null)
    {
        return new MenuModelBuilder(text, enable, tooltip);
    }
}