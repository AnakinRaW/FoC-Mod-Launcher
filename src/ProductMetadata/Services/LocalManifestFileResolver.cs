using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sklavenwalker.ProductMetadata.Manifest;
using Validation;

namespace Sklavenwalker.ProductMetadata.Services;

public class LocalManifestFileResolver : IManifestFileResolver
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger? _logger;

    public LocalManifestFileResolver(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public IFileInfo GetManifest(Uri manifestPath)
    {
        var manifestFilePath = manifestPath.LocalPath;
        var fileInfo = _fileSystem.FileInfo.FromFileName(manifestFilePath);
        if (!fileInfo.Exists)
            throw new ManifestNotFoundException($"Could not find manifest at {fileInfo.FullName}");
        return fileInfo;
    }
}