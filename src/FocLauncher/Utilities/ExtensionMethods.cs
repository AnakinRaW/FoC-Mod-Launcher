using System;

namespace FocLauncher.Utilities;

internal static class ExtensionMethods
{
    internal static void ParseUnhandledExceptionObject(this UnhandledExceptionEventArgs args, out string message)
    {
        var str = args.ExceptionObject is Exception exceptionObject ? exceptionObject.ToString() : null;
        message = $"Unhandled exception has occurred: {str}";
    }
}