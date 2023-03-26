using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.AppUpdaterFramework.Conditions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.ExternalUpdater.CLI;
using AnakinRaW.ExternalUpdater.CLI.Arguments;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.AppUpdaterFramework.ExternalUpdater;

internal class ExternalUpdaterService : IExternalUpdaterService
{
    private const string Identity = "ExternalUpdater";
    private const string ComponentName = "External Updater";

    private readonly IFileSystem _fileSystem;
    private readonly IProductService _productService;
    private readonly IExternalUpdaterLauncher _launcher;

    public string UpdaterIdentity => Identity;

    public ExternalUpdaterService(IServiceProvider serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _productService = serviceProvider.GetRequiredService<IProductService>();
        _launcher = serviceProvider.GetRequiredService<IExternalUpdaterLauncher>();
    }

    public IInstallableComponent GetExternalUpdaterComponent(Stream assemblyStream, string installDirectory)
    {
        var assemblyInformation = ExternalUpdaterInformation.FromAssemblyStream(assemblyStream);
       
        var fileVersion = assemblyInformation.FileVersion;
        var filePath = _fileSystem.Path.Combine(installDirectory, ExternalUpdaterConstants.AppUpdaterModuleName);
        var identity = new ProductComponentIdentity(UpdaterIdentity, assemblyInformation.InformationalVersion);

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

    public UpdateArguments CreateUpdateArguments()
    {
        return null;
    }

    public RestartArguments CreateRestartArguments(bool elevate)
    {
        return null;
    }

    public IFileInfo GetExternalUpdater()
    {
        if (_productService.GetInstalledComponents().Items.FirstOrDefault(c => c.Id == UpdaterIdentity) is not SingleFileComponent updaterComponent)
            throw new NotSupportedException("External updater component not registered to current product.");

        return updaterComponent.GetFile(_fileSystem, _productService.GetCurrentInstance().Variables);
    }

    public void Launch(ExternalUpdaterArguments arguments)
    {
        var updater = GetExternalUpdater();
        _launcher.Start(updater, arguments);
    }
}