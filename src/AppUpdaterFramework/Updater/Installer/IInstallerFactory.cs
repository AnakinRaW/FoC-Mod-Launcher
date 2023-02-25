using AnakinRaW.AppUpaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpaterFramework.Updater.Installer;

internal interface IInstallerFactory
{
    IInstaller CreateInstaller(IInstallableComponent component);
}