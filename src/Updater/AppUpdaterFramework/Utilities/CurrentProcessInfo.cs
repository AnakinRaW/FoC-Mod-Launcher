using System.Diagnostics;
using System;
#if !NET6_0_OR_GREATER
using Vanara.PInvoke;
#endif


namespace AnakinRaW.AppUpdaterFramework.Utilities;

internal class CurrentProcessInfo
{
    public static readonly CurrentProcessInfo Current = new();

    public string ProcessFilePath;

    public int Id;

    private CurrentProcessInfo()
    {
        var p = Process.GetCurrentProcess();
        Id = p.Id;
#if NET6_0
        var processPath = Environment.ProcessPath;
#else
        var processPath = Kernel32.GetModuleFileName(HINSTANCE.NULL);
#endif
        if (string.IsNullOrEmpty(processPath))
            throw new InvalidOperationException("Unable to get current process path");
        ProcessFilePath = processPath;
    }
}