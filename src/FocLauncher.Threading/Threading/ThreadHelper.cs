using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.VisualStudio.Threading;

namespace FocLauncher.Threading
{
    public abstract class ThreadHelper
    {
        private static Dispatcher _uiThreadDispatcher;
        private static ThreadHelper _generic;
        private static JoinableTaskContext _joinableTaskContextCache;

        public static JoinableTaskContext JoinableTaskContext => _joinableTaskContextCache ??= new JoinableTaskContext(Thread.CurrentThread,
            new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher, DispatcherPriority.Background));

        public static ThreadHelper Generic => _generic ??= new GenericThreadHelper();

        public static JoinableTaskFactory JoinableTaskFactory => JoinableTaskContext.Factory;

        private static Dispatcher DispatcherForUiThread
        {
            get
            {
                if (_uiThreadDispatcher == null && Application.Current != null)
                    _uiThreadDispatcher = Application.Current.Dispatcher;
                return _uiThreadDispatcher;
            }
        }

        static ThreadHelper()
        {
            SetUiThread();
        }

        /// <summary>
        ///     Determines whether the call is being made on the UI thread.
        /// </summary>
        /// <returns>Returns <see langword="true" /> if the call is on the UI thread, otherwise returns <see langword="false" />.</returns>
        public static bool CheckAccess()
        {
            var dispatcherForUiThread = DispatcherForUiThread;
            return dispatcherForUiThread != null && dispatcherForUiThread.CheckAccess();
        }

        public void BeginInvoke(Action action)
        {
            this.BeginInvoke(DispatcherPriority.Normal, action);
        }

        public void BeginInvoke(DispatcherPriority priority, Action action)
        {
            Dispatcher dispatcherForUiThread = ThreadHelper.DispatcherForUiThread;
            if (dispatcherForUiThread == null)
                throw new InvalidOperationException("Dispatcher unavailable");
            dispatcherForUiThread.BeginInvoke(priority, (Delegate)action);
        }

        public void Invoke(Action action)
        {
            using (GetInvocationWrapper())
            {
                if (CheckAccess())
                    action();
                else
                {
                    using var eventHandle = new ManualResetEventSlim(false);
                    ThreadHelper.Generic.BeginInvoke(() =>
                    {
                        action();
                        eventHandle.Set();
                    });
                    eventHandle.Wait();
                }
            }
        }

        public static void ThrowIfNotOnUIThread([CallerMemberName] string callerMemberName = "")
        {
            if (!CheckAccess())
                throw new COMException(string.Format(CultureInfo.CurrentCulture, "{0} must be called on the UI thread.",
                    new object[]
                    {
                        callerMemberName
                    }), -2147417842);
        }

        public static void ThrowIfOnUIThread([CallerMemberName] string callerMemberName = "")
        {
            if (CheckAccess())
                throw new COMException(string.Format(CultureInfo.CurrentCulture,
                    "{0} must be called on a background thread.", new object[]
                    {
                        callerMemberName
                    }), -2147417842);
        }

        internal static void SetUiThread()
        {
            _uiThreadDispatcher = Dispatcher.CurrentDispatcher;
        }

        protected abstract IDisposable GetInvocationWrapper();
    }
    
}
