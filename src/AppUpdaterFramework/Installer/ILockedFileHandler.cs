using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Installer;

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