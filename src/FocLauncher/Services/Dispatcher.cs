using System;
using System.Windows;
using System.Windows.Threading;

namespace FocLauncher.Services;

internal static class AppDispatcher
{
    private static readonly Dispatcher Dispatcher = Application.Current.Dispatcher;

    public static void Invoke(Action action) => InvokeInternal(action);

    public static T Invoke<T>(Func<T> func) => (T)InvokeInternal(func);

    public static void BeginInvoke(DispatcherPriority priority, Action action) => Dispatcher.InvokeAsync(action, priority);

    private static object InvokeInternal(Delegate @delegate)
    {
        return !Dispatcher.CheckAccess() ? Dispatcher.Invoke(@delegate) : @delegate.DynamicInvoke();
    }
}