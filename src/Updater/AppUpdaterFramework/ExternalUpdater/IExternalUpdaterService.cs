using System.IO;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.ExternalUpdater.CLI.Arguments;

namespace AnakinRaW.AppUpdaterFramework.ExternalUpdater;

public interface IExternalUpdaterService
{
    string UpdaterIdentity { get; }

    IInstallableComponent GetExternalUpdaterComponent(Stream assemblyStream, string installDirectory);

    UpdateArguments CreateUpdateArguments();

    RestartArguments CreateRestartArguments(bool elevate = false);

    IFileInfo GetExternalUpdater();

    void Launch(ExternalUpdaterArguments arguments);
}