using AnakinRaW.AppUpaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpaterFramework.Installer;

internal interface IInstallerFactory
{
    IInstaller CreateInstaller(IInstallableComponent component);
}