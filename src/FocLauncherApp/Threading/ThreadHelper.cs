using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace FocLauncherApp.Threading
{
    public class ThreadHelper
    {
        private static Dispatcher _uiThreadDispatcher;
        private static ThreadHelper _generic;

        public static ThreadHelper Generic => _generic ??= new ThreadHelper();
        
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
        
        public static bool CheckAccess()
        {
            var dispatcherForUiThread = DispatcherForUiThread;
            return dispatcherForUiThread != null && dispatcherForUiThread.CheckAccess();
        }

        public void Invoke(Action action)
        {
            if (CheckAccess())
                action();
        }

        public static void ThrowIfNotOnUIThread([CallerMemberName] string callerMemberName = "")
        {
            if (!CheckAccess())
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "{0} must be called on the UI thread.", new object[] {callerMemberName}));
        }

        public static void ThrowIfOnUIThread([CallerMemberName] string callerMemberName = "")
        {
            if (CheckAccess())
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "{0} must be called on a background thread.", new object[] {callerMemberName}));
        }

        internal static void SetUiThread()
        {
            _uiThreadDispatcher = Dispatcher.CurrentDispatcher;
        }
    }
}
