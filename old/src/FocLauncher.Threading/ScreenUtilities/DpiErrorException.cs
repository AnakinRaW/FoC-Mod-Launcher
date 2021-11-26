using System;

namespace FocLauncher.ScreenUtilities
{
    public class DpiErrorException : InvalidOperationException
    {
        public DpiErrorException(Dpi dpi, string message)
        {
        }
    }
}