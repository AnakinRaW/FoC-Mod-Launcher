using System.Collections.Generic;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Builder;

internal class MenuModelBuilder : CommandBarModelBuilder<IMenuDefinition>
{
    private readonly string _text;
    private readonly bool _enabled;
    private readonly string? _tooltip;

    public MenuModelBuilder(string text, bool enabled = true, string? tooltip = null)
    {
        _text = text;
        _enabled = enabled;
        _tooltip = tooltip;
    }

    protected override IMenuDefinition BuildCore(IReadOnlyList<ICommandBarGroup> groups)
    {
        return new MenuDefinition(_text, _enabled, _tooltip, groups);
    }
}