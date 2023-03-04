using AnakinRaW.AppUpdaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpdaterFramework.Installer;

internal struct InstallerInteractionResult
{
    public bool Retry { get; }

    public InstallResult InstallResult { get; }

    public InstallerInteractionResult(InstallResult installResult, bool retry = false)
    {
        Retry = retry;
        InstallResult = installResult;
    }
}