using System.Windows;
using System.Windows.Input;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Controls;

public class WorkerThreadStatusBarContainer : WorkerThreadElementContainer
{

    public static readonly DependencyProperty StatusBarFactoryProperty = DependencyProperty.Register(
        nameof(StatusBarFactory), typeof(IThreadedStatusBarFactory), typeof(WorkerThreadStatusBarContainer), new PropertyMetadata(default(IThreadedStatusBarFactory)));

    public IThreadedStatusBarFactory StatusBarFactory
    {
        get => (IThreadedStatusBarFactory) GetValue(StatusBarFactoryProperty);
        set => SetValue(StatusBarFactoryProperty, value);
    }

    private FrameworkElement _statusBarElement = null!;

    private IHasMouseEvents? StatusBarWithMouseEvent { get; set; }

    protected override string DispatcherGroup => "StatusBarModel";

    protected override int StackSize => 262144;
    
    protected override FrameworkElement CreateRootUiElement()
    {
        _statusBarElement = StatusBarFactory.CreateStatusBar();
        var dataContext = Dispatcher.Invoke(() => DataContext);
        StatusBarWithMouseEvent = _statusBarElement as IHasMouseEvents;
        _statusBarElement.DataContext = dataContext;
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