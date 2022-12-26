using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Sklavenwalker.CommonUtilities.Wpf.Utilities;

internal class MenuUtilities
{
    internal static void ProcessForDirectionalNavigation(KeyEventArgs e, ItemsControl itemsControl, Orientation orientation)
    {
        if (e.Handled)
            return;
        switch (CorrectKeysForNavigation(e.Key, itemsControl.FlowDirection, orientation))
        {
            case Key.Back:
                if (FocusManager.GetFocusedElement(itemsControl) is not FrameworkElement focusedElement)
                    break;
                e.Handled = focusedElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                break;
            case Key.End:
                e.Handled = GetNavigationContainer(itemsControl).MoveFocus(new TraversalRequest(FocusNavigationDirection.Last));
                break;
            case Key.Home:
                e.Handled = GetNavigationContainer(itemsControl).MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                break;
            case Key.Left:
                if (orientation != Orientation.Horizontal || FocusManager.GetFocusedElement(itemsControl) is not FrameworkElement focusedElement2)
                    break;
                e.Handled = focusedElement2.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                break;
            case Key.Right:
                if (orientation != Orientation.Horizontal || FocusManager.GetFocusedElement(itemsControl) is not FrameworkElement focusedElement3)
                    break;
                e.Handled = focusedElement3.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                break;
        }
    }

    private static UIElement GetNavigationContainer(ItemsControl itemsControl)
    {
        if (itemsControl is MenuItem templatedParent && 
            templatedParent.Template.FindName("PART_Popup", templatedParent) is Popup { Child: { } } name)
            return name.Child;
        return itemsControl;
    }

    internal static Key CorrectKeysForNavigation(Key key, FlowDirection flowDirection, Orientation orientation)
    {
        if (flowDirection == FlowDirection.RightToLeft && orientation == Orientation.Horizontal)
        {
            switch (key)
            {
                case Key.End:
                    return Key.Home;
                case Key.Home:
                    return Key.End;
                case Key.Left:
                    return Key.Right;
                case Key.Right:
                    return Key.Left;
            }
        }
        return key;
    }
}