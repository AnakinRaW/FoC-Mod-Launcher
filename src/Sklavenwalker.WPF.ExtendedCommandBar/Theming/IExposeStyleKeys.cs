using System.Windows;

namespace Sklavenwalker.Wpf.CommandBar.Theming;

internal interface IExposeStyleKeys
{
    ResourceKey ButtonStyleKey { get; }

    ResourceKey MenuControllerStyleKey { get; }

    ResourceKey ComboBoxStyleKey { get; }

    ResourceKey MenuStyleKey { get; }

    ResourceKey SeparatorStyleKey { get; }
}