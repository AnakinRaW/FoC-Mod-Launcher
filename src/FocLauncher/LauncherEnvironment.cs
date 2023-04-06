using System.IO.Abstractions;
using System.Reflection;
using AnakinRaW.ApplicationBase;
using Flurl;

namespace FocLauncher;

internal class LauncherEnvironment : ApplicationEnvironmentBase
{
    public const string LauncherLogDirectoryName = "FocLauncher_Logs";

    public override string ApplicationName => "Foc Mod Launcher";
    public override Url? RepositoryUrl { get; } = new("https://github.com/AnakinRaW/FoC-Mod-Launcher");
    public override Url UpdateRootUrl { get; } = new("https://republicatwar.com/downloads/FocLauncher/v2");
    protected override string ApplicationLocalDirectoryName => "FocLauncher";

    public LauncherEnvironment(Assembly assembly, IFileSystem fileSystem) : base(assembly, fileSystem)
    {
    }
}