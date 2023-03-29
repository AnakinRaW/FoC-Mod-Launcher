using System.IO;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.ExternalUpdater.Options;

namespace AnakinRaW.AppUpdaterFramework.ExternalUpdater;

public interface IExternalUpdaterService
{
    string UpdaterIdentity { get; }

    IInstallableComponent GetExternalUpdaterComponent(Stream assemblyStream, string installDirectory);

    UpdateOptions CreateUpdateOptions();

    RestartOptions CreateRestartOptions(bool elevate = false);

    IFileInfo GetExternalUpdater();

    void Launch(ExternalUpdaterOptions options);
}