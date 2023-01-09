using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.Utilities;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public class ThemedMenuItem : MenuItem, INonClientArea
{
    public static readonly RoutedEvent CommandExecutedRoutedEvent =
        EventManager.RegisterRoutedEvent("CommandExecuted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ThemedMenuItem));

    private Popup? _popup;

    public static double MaxMenuWidth => 660.0;

    static ThemedMenuItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedMenuItem), new FrameworkPropertyMetadata(typeof(ThemedMenuItem)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _popup = GetTemplateChild("PART_Popup") as Popup;
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        if (oldDpi.DpiScaleX != newDpi.DpiScaleX || oldDpi.DpiScaleY != newDpi.DpiScaleY)
        {
            UtilityMethods.InvalidateRecursiveToType<SystemDropShadowChrome>(_popup?.Child);
            UtilityMethods.InvalidateRecursiveToType<Grid>(this);
        }
        base.OnDpiChanged(oldDpi, newDpi);
    }

    protected override void OnSubmenuOpened(RoutedEventArgs e)
    {
        UtilityMethods.InvalidateRecursive(_popup?.Child);
        base.OnSubmenuOpened(e);
    }

    protected override void OnClick()
    {
        base.OnClick();
        RaiseEvent(new RoutedEventArgs(CommandExecutedRoutedEvent, this));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // TODO: Return if ToolbarHosted
        if (IsSubmenuOpen)
            MenuUtilities.ProcessForDirectionalNavigation(e, this, Orientation.Vertical);
        base.OnKeyDown(e);
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ThemedMenuItem();
    }

    int INonClientArea.HitTest(Point point)
    {
        return 1;
    }
}