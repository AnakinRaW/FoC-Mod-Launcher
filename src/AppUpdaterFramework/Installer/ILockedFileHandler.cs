using System.IO.Abstractions;
using AnakinRaW.AppUpaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpaterFramework.Installer;

internal interface ILockedFileHandler
{
    Result Handle(IInstallableComponent component, IFileInfo file);

    public enum Result
    {
        Unlocked,
        Locked,
        RequiresRestart
    }
}