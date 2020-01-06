using System;

namespace FocLauncherApp.ScreenUtilities
{
    public class MonitorDpiAwarenessException : Win32DpiAwarenessException
    {
        public MonitorDpiAwarenessException(IntPtr hmon, int hr, double invalidDpi, string message)
            : base(hr, invalidDpi, message)
        {
            MonitorHandle = hmon;
        }

        public IntPtr MonitorHandle { get; }
    }
}