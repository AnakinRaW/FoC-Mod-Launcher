using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FocLauncher.Utilities;

internal class ResourceExtractor : ICosturaResourceExtractor
{
    private readonly ILogger? _logger;
    private readonly IFileSystem _fileSystem;

    private readonly IReadOnlyCollection<string> _assemblyResourceNames;

    public ResourceExtractor(Assembly assembly, IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _assemblyResourceNames = assembly.GetManifestResourceNames();
    }

    public async Task ExtractAsync(string resourceName, string fileDirectory)
    {
        await ExtractAsync(resourceName, fileDirectory, (_, _) => true);
    }

    public async Task ExtractAsync(string resourceName, string fileDirectory, Func<string, Stream, bool> shouldOverwrite)
    {
        foreach (var rn in _assemblyResourceNames)
        {
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(rn, resourceName, CompareOptions.IgnoreCase) < 0)
                continue;
            var compressed = rn.EndsWith(".compressed");
            using var assemblyStream = await GetResourceStream(rn, compressed);
            await ExtractAsync(assemblyStream, resourceName, fileDirectory, shouldOverwrite);
        }
    }

    public async Task<Stream> GetResourceAsync(string assemblyName)
    {
        foreach (var resourceName in _assemblyResourceNames)
        {
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(resourceName, assemblyName, CompareOptions.IgnoreCase) < 0)
                continue;
            var compressed = resourceName.EndsWith(".compressed");
            return await GetResourceStream(resourceName, compressed);
        }

        throw new IOException($"Could not find embedded resource '{assemblyName}'");
    }

    private static async Task<Stream> GetResourceStream(string resourceName, bool compressed)
    {
        using var assemblyResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (assemblyResourceStream is null)
            throw new InvalidOperationException($"Assembly stream for '{resourceName}' was null!");

        Stream assemblyStream;
        if (compressed)
            assemblyStream = await DecompressAsync(assemblyResourceStream);
        else
        {
            assemblyStream = new MemoryStream();
            await assemblyResourceStream.CopyToAsync(assemblyStream);
        }

        assemblyStream.Position = 0;
        return assemblyStream;
    }

    private async Task ExtractAsync(Stream resourceStream, string assemblyName, string fileDirectory, Func<string, Stream, bool> shouldOverwrite)
    {
        if (!_fileSystem.Directory.Exists(fileDirectory))
            throw new DirectoryNotFoundException("The requested destination folder does not exist.");

        var filePath = _fileSystem.Path.Combine(fileDirectory, assemblyName);
        try
        {
            if (_fileSystem.File.Exists(filePath) && !shouldOverwrite(filePath, resourceStream))
                return;

            _logger?.LogDebug($"Writing file: '{filePath}'");
            await WriteToFileAsync(resourceStream, filePath);
        }
        catch (Exception ex)
        {
            throw new IOException("Error writing necessary launcher files to disk!", ex);
        }
    }

    private static async Task WriteToFileAsync(Stream assemblyStream, string filePath)
    {
        assemblyStream.Position = 0;
        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await assemblyStream.CopyToAsync(fs);
    }

    private static async Task<MemoryStream> DecompressAsync(Stream stream)
    {
        stream.Position = 0;
        using var decompressionStream = new DeflateStream(stream, CompressionMode.Decompress);
        var memoryStream = new MemoryStream();
        await decompressionStream.CopyToAsync(memoryStream);
        return memoryStream;
    }
}