using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace FocLauncherApp.WaitDialog
{
    internal sealed class BackgroundDispatcher
    {
        private static readonly List<BackgroundDispatcher> Dispatchers = new List<BackgroundDispatcher>();
        private readonly string _name;



        private Dispatcher Dispatcher { get; set; }

        private BackgroundDispatcher(string name, int stackSize)
        {
            _name = name;
            CreateDispatcher(stackSize);
        }

        public static Dispatcher GetBackgroundDispatcher(string dispatcherName, int stackSize = 0)
        {
            lock (Dispatchers)
            {
                foreach (var dispatcher in Dispatchers)
                {
                    if (dispatcher._name == dispatcherName)
                        return dispatcher.Dispatcher;
                }
                var backgroundDispatcher = new BackgroundDispatcher(dispatcherName, stackSize);
                Dispatchers.Add(backgroundDispatcher);
                return backgroundDispatcher.Dispatcher;
            }
        }

        private void CreateDispatcher(int stackSize)
        {
            Thread thread = new Thread(ThreadProc, stackSize)
            {
                IsBackground = true,
                CurrentCulture = CultureInfo.CurrentCulture,
                CurrentUICulture = CultureInfo.CurrentUICulture,
                Name = _name
            };
            thread.SetApartmentState(ApartmentState.STA);
            var manualResetEvent = new ManualResetEvent(false);
            thread.Start(manualResetEvent);
            manualResetEvent.WaitOne();
            HookEvents();
        }

        private void HookEvents()
        {
            Application.Current.Exit += OnApplicationExit;
            Dispatcher.CurrentDispatcher.ShutdownStarted += OnDispatcherShutdownStarted;
        }

        private void UnhookEvents()
        {
            Application.Current.Exit -= OnApplicationExit;
            Dispatcher.CurrentDispatcher.ShutdownStarted -= OnDispatcherShutdownStarted;
        }

        private void OnDispatcherShutdownStarted(object sender, EventArgs e)
        {
            HandleTerminationEvent();
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            HandleTerminationEvent();
        }

        private void HandleTerminationEvent()
        {
            Dispatcher.InvokeShutdown();
            UnhookEvents();
        }

        [DebuggerStepThrough]
        private void ThreadProc(object arg)
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            ((EventWaitHandle) arg).Set();
            try
            {
                Dispatcher.Run();
            }
            catch (Win32Exception)
            {

            }
            finally
            {
                Dispatcher = null;
            }
        }
    }
}
