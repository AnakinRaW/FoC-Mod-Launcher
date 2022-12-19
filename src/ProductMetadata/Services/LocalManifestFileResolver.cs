using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.ProductMetadata.Catalog;
using Validation;

namespace Sklavenwalker.ProductMetadata.Services;

public class LocalManifestFileResolver : IManifestFileResolver
{
    private readonly IFileSystem _fileSystem;

    public LocalManifestFileResolver(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    }

    public Task<IFileInfo> GetManifest(Uri manifestPath, CancellationToken token)
    {
        var manifestFilePath = manifestPath.LocalPath;
        var fileInfo = _fileSystem.FileInfo.FromFileName(manifestFilePath);
        if (!fileInfo.Exists)
            throw new CatalogDownloadException($"Could not find manifest at {fileInfo.FullName}");
        return Task.FromResult(fileInfo);
    }
}