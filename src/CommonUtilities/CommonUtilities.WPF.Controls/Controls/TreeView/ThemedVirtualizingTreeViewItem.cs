using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public class ThemedVirtualizingTreeViewItem : VirtualizingTreeViewItem
{
    static ThemedVirtualizingTreeViewItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedVirtualizingTreeViewItem),
            new FrameworkPropertyMetadata(typeof(ThemedVirtualizingTreeViewItem)));
    }
}