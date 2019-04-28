using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace FocLauncherApp.Threading
{
    public abstract class ThreadHelper
    {
        private static Dispatcher _uiThreadDispatcher;
        private static ThreadHelper _generic;


        public static ThreadHelper Generic => _generic ?? (_generic = new GenericThreadHelper());

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

        public void Invoke(Action action)
        {
            using (GetInvocationWrapper())
            {
                if (CheckAccess())
                    action();
                else
                {
                    //action.OnUIThread();
                    Application.Current?.Dispatcher.Invoke(action);
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

        internal static void SetUiThread()
        {
            _uiThreadDispatcher = Dispatcher.CurrentDispatcher;
        }

        protected abstract IDisposable GetInvocationWrapper();
    }
}
