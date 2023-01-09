using System.Windows.Controls;
using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.Controls;

namespace FocLauncher.Controls;

internal sealed class LauncherTreeViewControl : ThemedVirtualizingTreeView
{
    public LauncherTreeViewControl()
    {
        IsExpandablePath = "HasItems";
        ItemsPath = "Items";
        SelectionMode = SelectionMode.Single;
        HierarchyIndention = 18;
        AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown), true);
        KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Cycle);
        KeyboardNavigation.SetDirectionalNavigation(this, KeyboardNavigationMode.Contained);
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (IsKeyboardFocusWithin || Mouse.Captured != null || e.ClickCount != 1)
            return;
        Focus();
    }
}