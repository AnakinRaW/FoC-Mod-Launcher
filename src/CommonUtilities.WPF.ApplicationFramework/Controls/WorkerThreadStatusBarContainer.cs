using System.Windows;
using System.Windows.Input;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Controls;

internal class WorkerThreadStatusBarContainer : WorkerThreadElementContainer
{
    private readonly IStatusBarFactory _factory;

    private UIElement _statusBarElement = null!;

    private IHasMouseEvents? StatusBarWithMouseEvent { get; set; }

    protected override string DispatcherGroup => "StatusBar";

    protected override int StackSize => 262144;

    public WorkerThreadStatusBarContainer(IStatusBarFactory factory)
    {
        Requires.NotNull(factory, nameof(factory));
        _factory = factory;
    }

    protected override UIElement CreateRootUiElement()
    {
        _statusBarElement = _factory.CreateStatusBar();
        StatusBarWithMouseEvent = _statusBarElement as IHasMouseEvents;
        return _statusBarElement;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (StatusBarWithMouseEvent is null)
            return;
        var point = e.GetPosition(this);
        _statusBarElement.Dispatcher.BeginInvoke(() => StatusBarWithMouseEvent.Click(point));
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (StatusBarWithMouseEvent is null)
            return;
        var point = e.GetPosition(this);
        _statusBarElement.Dispatcher.BeginInvoke(() => StatusBarWithMouseEvent.MouseMovePosition(point));
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (StatusBarWithMouseEvent is null)
            return;
        var point = e.GetPosition(this);
        _statusBarElement.Dispatcher.BeginInvoke(() => StatusBarWithMouseEvent.MouseLeaveControl(point));
    }

    protected override bool ShouldForwardPropertyChange(DependencyPropertyChangedEventArgs e)
    {
        return e.Property != DataContextProperty && base.ShouldForwardPropertyChange(e);
    }
}