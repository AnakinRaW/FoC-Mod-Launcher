using System;
using System.Windows.Threading;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;

internal class AppDispatcher : IAppDispatcher
{
    private readonly Dispatcher _dispatcher;

    public AppDispatcher(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public void Invoke(Action action)
    {
        InvokeInternal(action);
    }

    public T Invoke<T>(Func<T> func)
    {
        return (T)InvokeInternal(func);
    }

    public void BeginInvoke(DispatcherPriority priority, Action action)
    {
        _dispatcher.InvokeAsync(action, priority);
    }

    private object InvokeInternal(Delegate @delegate)
    {
        return !_dispatcher.CheckAccess() ? _dispatcher.Invoke(@delegate) : @delegate.DynamicInvoke();
    }
}