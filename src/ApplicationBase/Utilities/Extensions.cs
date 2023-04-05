using System;

namespace AnakinRaW.ApplicationBase.Utilities;

internal static class Extensions
{
    internal static void ParseUnhandledExceptionObject(this UnhandledExceptionEventArgs args, out string message)
    {
        var str = args.ExceptionObject is Exception exceptionObject ? exceptionObject.ToString() : null;
        message = $"Unhandled exception has occurred: {str}";
    }
}