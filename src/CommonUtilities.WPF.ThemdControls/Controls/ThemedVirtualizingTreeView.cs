using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class ThemedVirtualizingTreeView : VirtualizingTreeView
{
    static ThemedVirtualizingTreeView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedVirtualizingTreeView),
            new FrameworkPropertyMetadata(typeof(ThemedVirtualizingTreeView)));
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is ThemedVirtualizingTreeViewItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ThemedVirtualizingTreeViewItem();
    }
}