using System;
using FocLauncherHost.Updater.MetadataModel;

namespace FocLauncherHost.Updater
{
    public interface IDependency
    {
        InstallLocation InstallLocation { get; set; }
        bool RequiresRestart { get; set; }
        string SourceLocation { get; set; }
        byte[]? Sha2 { get; set; }
        string Name { get; set; }
        string Version { get; set; }
        DependencyAction RequiredAction { get; set; }
        CurrentDependencyState CurrentState { get; set; }
        Version? GetVersion();
    }
}