using System;
using System.Collections.Generic;
using System.Linq;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Builder;

internal abstract class CommandBarModelBuilder<T> : 
    ICommandBarBuilderStartContext<T>, ICommandBarBuilderContext<T>, ICommandBarBuilderSeparatorContext<T>
    where T : class, ICommandBarItemsSource
{
    private readonly List<ICommandBarGroup> _groups = new();
    private readonly List<ICommandBarItemDefinition> _currentGroupItems = new();

    public T Build()
    {
        CreateGroup();
        var definition = BuildCore(_groups.ToList());
        _groups.Clear();
        return definition;
    }

    protected abstract T BuildCore(IReadOnlyList<ICommandBarGroup> groups);

    public ICommandBarBuilderSeparatorContext<T> AddSeparator()
    {
        CreateGroup();
        return this;
    }

    private void CreateGroup()
    {
        if (!_currentGroupItems.Any())
            throw new InvalidOperationException("Cannot create empty group");
        var group = new CommandBarGroup(_currentGroupItems.ToList());
        _groups.Add(group);
        _currentGroupItems.Clear();
    }

    private ICommandBarBuilderContext<T> AddItemInternal(ICommandBarItemDefinition item)
    {
        _currentGroupItems.Add(item);
        return this;
    }

    ICommandBarBuilderContext<T> ICommandBarBuilderContext<T>.AddItem(ICommandBarItemDefinition item)
    {
        return AddItemInternal(item);
    }

    ICommandBarBuilderContext<T> ICommandBarBuilderSeparatorContext<T>.AddItem(ICommandBarItemDefinition item)
    {
        return AddItemInternal(item);
    }

    ICommandBarBuilderContext<T> ICommandBarBuilderStartContext<T>.AddItem(ICommandBarItemDefinition item)
    {
        return AddItemInternal(item);
    }
}