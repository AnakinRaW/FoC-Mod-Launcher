using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Utilities;

public sealed class BackgroundDispatcher
{
    private readonly string _name;
    private static readonly List<BackgroundDispatcher> SDispatchers = new();

    private Dispatcher Dispatcher { get; set; }
    
    private BackgroundDispatcher(string name, int stackSize)
    {
        _name = name;
        CreateDispatcher(stackSize);
    }

    public static Dispatcher GetBackgroundDispatcher(string dispatcherName, int stackSize = 0)
    {
        Requires.NotNullOrEmpty(dispatcherName, nameof(dispatcherName));
        lock (SDispatchers)
        {
            foreach (var dispatcher in SDispatchers)
            {
                if (dispatcher._name == dispatcherName)
                    return dispatcher.Dispatcher;
            }
            var backgroundDispatcher = new BackgroundDispatcher(dispatcherName, stackSize);
            SDispatchers.Add(backgroundDispatcher);
            return backgroundDispatcher.Dispatcher;
        }
    }

    private void CreateDispatcher(int stackSize)
    {
        var thread = new Thread(ThreadProc, stackSize)
        {
            IsBackground = true,
            CurrentCulture = CultureInfo.CurrentCulture,
            CurrentUICulture = CultureInfo.CurrentUICulture,
            Name = _name
        };
        thread.SetApartmentState(ApartmentState.STA);
        var parameter = new ManualResetEvent(false);
        thread.Start(parameter);
        parameter.WaitOne();
        HookEvents();
    }

    private void OnApplicationExit(object sender, ExitEventArgs e) => HandleTerminationEvent();

    private void OnDispatcherShutdownStarted(object sender, EventArgs e) => HandleTerminationEvent();

    private void HandleTerminationEvent()
    {
        Dispatcher.InvokeShutdown();
        UnhookEvents();
    }

    private void HookEvents()
    {
        if (Application.Current != null)
            Application.Current.Exit += OnApplicationExit;
        Dispatcher.CurrentDispatcher.ShutdownStarted += OnDispatcherShutdownStarted;
    }

    private void UnhookEvents()
    {
        if (Application.Current != null)
            Application.Current.Exit -= OnApplicationExit;
        Dispatcher.CurrentDispatcher.ShutdownStarted -= OnDispatcherShutdownStarted;
    }

    private void ThreadProc(object arg)
    {
        Dispatcher = Dispatcher.CurrentDispatcher;
        ((EventWaitHandle)arg).Set();
        Dispatcher.Run();
        Dispatcher = null;
    }
}