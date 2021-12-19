using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class WorkerThreadStatusBarContainer : WorkerThreadElementContainer, IPointConverter
{
    private readonly IStatusBarFactory _factory;

    private UIElement _statusBarElement;

    private IHasMouseEvents? StatusBarWithMouseEvent { get; set; }

    protected override string DispatcherGroup => "StatusBar";

    protected override int StackSize => 262144;

    public WorkerThreadStatusBarContainer(IStatusBarFactory factory)
    {
        Requires.NotNull(factory, nameof(factory));
        _factory = factory;
    }

    public async Task<Point> PointToScreenCoordinatesAsync(Point point)
    {
        return await Dispatcher.InvokeAsync(() => PointToScreen(point));
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