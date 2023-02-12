using System;
using System.Threading.Tasks;

namespace FocLauncher.Utilities;

internal static class Extensions
{
    internal static void ParseUnhandledExceptionObject(this UnhandledExceptionEventArgs args, out string message)
    {
        var str = args.ExceptionObject is Exception exceptionObject ? exceptionObject.ToString() : null;
        message = $"Unhandled exception has occurred: {str}";
    }

    public static void Forget(this Task task)
    {
        // Intentionally empty
    }
}