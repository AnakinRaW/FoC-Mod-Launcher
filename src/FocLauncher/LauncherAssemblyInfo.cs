using System.Diagnostics;
using System.Reflection;
using Semver;

namespace FocLauncher;

internal static class LauncherAssemblyInfo
{
    internal static Assembly CurrentAssembly { get; }
    internal static string InformationalVersion { get; }

    internal static string FileVersion { get; }

    internal static string AssemblyVersion { get; }
    
    internal static string Title { get; }

    internal static string AssemblyName { get; }

    static LauncherAssemblyInfo()
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        CurrentAssembly = executingAssembly;
        InformationalVersion = executingAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        FileVersion = FileVersionInfo.GetVersionInfo(executingAssembly.Location).FileVersion;
        AssemblyVersion = executingAssembly.GetName().Version.ToString();
        Title = executingAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
        AssemblyName = executingAssembly.GetName().Name;
    }

    internal static SemVersion? InformationalAsSemVer()
    {
        SemVersion.TryParse(InformationalVersion, SemVersionStyles.Any, out var version);
        return version;
    }
}