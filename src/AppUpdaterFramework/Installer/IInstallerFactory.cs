using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Installer;

internal interface IInstallerFactory
{
    IInstaller CreateInstaller(IInstallableComponent component);
}