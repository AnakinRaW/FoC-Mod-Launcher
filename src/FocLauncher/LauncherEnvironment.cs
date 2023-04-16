using System;
using System.Collections.Generic;
using System.Reflection;
using AnakinRaW.ApplicationBase;

namespace FocLauncher;

internal class LauncherEnvironment : ApplicationEnvironmentBase
{
    public const string LauncherLogDirectoryName = "FocLauncher_Logs";

    public override string ApplicationName => "Foc Mod Launcher";

    public override Uri? RepositoryUrl { get; } = new("https://github.com/AnakinRaW/FoC-Mod-Launcher");

    public override ICollection<Uri> UpdateMirrors { get; } = new List<Uri>
    {
        new("https://republicatwar.com/downloads/FocLauncher")
    };

    public override string ApplicationRegistryPath => @"SOFTWARE\FocLauncher";
    protected override string ApplicationLocalDirectoryName => "FocLauncher";

    public LauncherEnvironment(Assembly assembly, IServiceProvider serviceProvider) : base(assembly, serviceProvider)
    {
    }
}