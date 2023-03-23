using System;
using System.IO;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Conditions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.ExternalUpdater.CLI;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.AppUpdaterFramework.ExternalUpdater;

internal class ExternalUpdaterService : IExternalUpdaterService
{
    private const string Identity = "ExternalUpdater";
    private const string ComponentName = "External Updater";

    private readonly IFileSystem _fileSystem;

    public ExternalUpdaterService(IServiceProvider serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    }

    public IInstallableComponent GetExternalUpdaterComponent(Stream assemblyStream, string installDirectory)
    {
        var assemblyInformation = ExternalUpdaterAssemblyInformation.FromAssemblyStream(assemblyStream);
       
        var fileVersion = assemblyInformation.FileVersion;
        var filePath = _fileSystem.Path.Combine(installDirectory, ExternalUpdaterConstants.AppUpdaterModuleName);
        var identity = new ProductComponentIdentity(Identity, assemblyInformation.InformationalVersion);

        return new SingleFileComponent(identity, installDirectory, ExternalUpdaterConstants.AppUpdaterModuleName, null)
        {
            Name = ComponentName,
            DetectConditions = new[]
            {
                new FileCondition(filePath)
                {
                    Version = fileVersion
                }
            }
        };
    }
}

public interface IExternalUpdaterService
{
    IInstallableComponent GetExternalUpdaterComponent(Stream assemblyStream, string installDirectory);
}