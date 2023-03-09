using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Semver;

namespace FocLauncher;

internal static class LauncherAssemblyInfo
{
    internal static Assembly CurrentAssembly { get; }

    internal static string ProductName { get; }

    internal static string InformationalVersion { get; }

    internal static string FileVersion { get; }

    internal static string AssemblyVersion { get; }
    
    internal static string Title { get; }

    internal static string ExecutableFileName { get; }

    static LauncherAssemblyInfo()
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        CurrentAssembly = executingAssembly;
        InformationalVersion = executingAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        var fv = FileVersionInfo.GetVersionInfo(executingAssembly.Location);
        FileVersion = fv.FileVersion;
        ProductName = fv.ProductName;
        AssemblyVersion = executingAssembly.GetName().Version.ToString();
        Title = executingAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
        ExecutableFileName = executingAssembly.Modules.First().Name;
    }

    internal static SemVersion? InformationalAsSemVer()
    {
        SemVersion.TryParse(InformationalVersion, SemVersionStyles.Any, out var version);
        return version;
    }
}