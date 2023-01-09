using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Controls;

public interface IHasCommandBarStyles
{
    ResourceKey ButtonStyleKey { get; }

    ResourceKey MenuStyleKey { get; }

    ResourceKey SeparatorStyleKey { get; }
}