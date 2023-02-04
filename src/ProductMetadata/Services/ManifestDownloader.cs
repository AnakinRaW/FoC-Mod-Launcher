using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.DownloadManager;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.ProductMetadata.Services;

internal class ManifestDownloader : IManifestDownloader
{
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemService _fileSystemHelper;
    private readonly IDownloadManager _downloadManager;
    private string _temporaryDownloadDirectory;

    private string TemporaryDownloadDirectory
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_temporaryDownloadDirectory) || !_fileSystem.Directory.Exists(_temporaryDownloadDirectory))
                _temporaryDownloadDirectory = _fileSystemHelper.CreateTemporaryFolderInTempWithRetry(10)?.FullName ??
                                              throw new IOException("Unable to create temporary directory");
            return _temporaryDownloadDirectory;
        }
    }

    public ManifestDownloader(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _fileSystemHelper = serviceProvider.GetRequiredService<IFileSystemService>();
        _downloadManager = serviceProvider.GetService<IDownloadManager>() ?? new DownloadManager(serviceProvider);
    }

    public async Task<IFileInfo> GetManifest(Uri manifestPath, CancellationToken token = default)
    {
        var destPath = CreateRandomFile();
        using var manifest = _fileSystem.FileStream.Create(destPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        await _downloadManager.DownloadAsync(manifestPath, manifest, null , null, token);
        return _fileSystem.FileInfo.FromFileName(destPath);
    }


    private string CreateRandomFile()
    {
        var location = TemporaryDownloadDirectory;
        string file;
        var count = 0;
        do
        {
            var fileName = _fileSystem.Path.GetRandomFileName();
            file = _fileSystem.Path.Combine(location, fileName);
        } while (_fileSystem.File.Exists(file) && count++ <= 10);
        if (count > 10)
            throw new IOException($"Unable to create temporary file under '{location}'");
        return file;
    }
}