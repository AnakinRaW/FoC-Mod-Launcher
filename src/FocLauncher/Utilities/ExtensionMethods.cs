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

internal class HexTools
{
    public static byte[] StringToByteArray(string hex)
    {
        var numberChars = hex.Length;
        var bytes = new byte[numberChars / 2];
        for (var i = 0; i < numberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }
}