using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Controls;

public interface IHasCommandBarStyles
{
    ResourceKey ButtonStyleKey { get; }

    ResourceKey MenuStyleKey { get; }

    ResourceKey SeparatorStyleKey { get; }
}