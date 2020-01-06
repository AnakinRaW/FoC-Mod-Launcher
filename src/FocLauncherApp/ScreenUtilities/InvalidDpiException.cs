using System;

namespace FocLauncherApp.ScreenUtilities
{
    public class InvalidDpiException : InvalidOperationException
    {
        public InvalidDpiException(double invalidDpi, string message)
            : base(message)
        {
            InvalidDpi = invalidDpi;
        }

        public double InvalidDpi { get; }
    }
}