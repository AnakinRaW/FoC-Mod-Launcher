using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FocLauncher.Services;

internal class ThreadHelper : IThreadHelper
{
    private static Dispatcher? _uiThreadDispatcher;

    private static Dispatcher DispatcherForUiThread
    {
        get
        {
            if (_uiThreadDispatcher is null)
            {
                if (Application.Current == null)
                    throw new InvalidOperationException("Dispatcher unavailable");
                _uiThreadDispatcher = Application.Current.Dispatcher;
            }
                
            return _uiThreadDispatcher;
        }
    }

    public void OnUIThread(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
    {
        if (Application.Current is null)
            throw new InvalidOperationException("No Application found");
        DispatcherForUiThread.BeginInvoke(priority, action);
    }

    public Task OnUIThread(Func<Task> action)
    {
        if (Application.Current is null)
            throw new InvalidOperationException("No Application found");
        return DispatcherForUiThread.Invoke(action);
    }
}