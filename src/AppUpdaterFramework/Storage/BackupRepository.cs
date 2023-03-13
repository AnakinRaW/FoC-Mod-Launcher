using System;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.AppUpdaterFramework.Storage;

internal sealed class BackupRepository : FileRepository
{
    protected override IDirectoryInfo Root { get; }

    protected override string FileExtensions => "bak";

    public BackupRepository(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        var updateConfig = serviceProvider.GetRequiredService<IUpdateConfigurationProvider>().GetConfiguration();

        var location = updateConfig.BackupLocation;
        if (string.IsNullOrEmpty(location))
            throw new InvalidOperationException("backup directory not specified.");

        var root = fileSystem.DirectoryInfo.New(location);
        root.Create();

        Root = root;
    }
}