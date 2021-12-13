﻿using System.Diagnostics;
using System.Reflection;

namespace FocLauncher;

internal static class LauncherAssemblyInfo
{
    internal static string InformationalVersion { get; }

    internal static string FileVersion { get; }
    
    internal static string Title { get; }

    internal static string AssemblyName { get; }


    static LauncherAssemblyInfo()
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        InformationalVersion = executingAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        FileVersion = FileVersionInfo.GetVersionInfo(executingAssembly.Location).FileVersion;
        Title = executingAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
        AssemblyName = executingAssembly.GetName().Name;
    }
}