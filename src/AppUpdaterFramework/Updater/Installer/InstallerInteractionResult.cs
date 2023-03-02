using AnakinRaW.AppUpaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpaterFramework.Updater.Installer;

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