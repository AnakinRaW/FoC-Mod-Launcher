using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.FileSystem.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FocLauncher.Utilities;

internal class FileVersionBasedAssemblyExtractor : ICosturaAssemblyExtractor
{
    private readonly ILogger? _logger;
    private readonly IWindowsPathService _pathHelper;
    private readonly IFileSystem _fileSystem;

    private readonly IReadOnlyCollection<string> _assemblyResourceNames;

    public FileVersionBasedAssemblyExtractor(Assembly assembly, IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _pathHelper = serviceProvider.GetRequiredService<IWindowsPathService>();
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _assemblyResourceNames = assembly.GetManifestResourceNames();
    }

    public Task ExtractAssembliesAsync(IEnumerable<string> assemblyNames, string fileDirectory)
    {
        var tasks = assemblyNames.Select(assemblyName => ExtractAssemblyAsync(assemblyName, fileDirectory)).ToList();
        return Task.WhenAll(tasks);
    }

    public async Task ExtractAssemblyAsync(string assemblyName, string fileDirectory)
    {
        foreach (var resourceName in _assemblyResourceNames)
        {
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(resourceName, assemblyName, CompareOptions.IgnoreCase) < 0)
                continue;
            var compressed = resourceName.EndsWith(".compressed");
            await ExtractAsync(resourceName, assemblyName, fileDirectory, compressed);
        }
    }

    private async Task ExtractAsync(string resourceName, string assemblyName, string fileDirectory, bool compressed)
    {
        if (!_fileSystem.Directory.Exists(fileDirectory))
            throw new DirectoryNotFoundException("The requested destination folder does not exist.");

        var filePath = _fileSystem.Path.Combine(fileDirectory, assemblyName);
        try
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
           
            if (_fileSystem.File.Exists(filePath) && !ShouldOverrideAssembly(filePath))
                return;

            _logger?.LogDebug($"Writing file: '{filePath}'");
            await WriteToFileAsync(assemblyStream, filePath);
        }
        catch (Exception ex)
        {
            throw new IOException("Error writing necessary launcher files to disk!", ex);
        }
    }

    private bool ShouldOverrideAssembly(string filePath)
    {
        // We, again, assume that the version of the embedded AppUpdater is equal to the version of the current assembly.
        var currentFileVersion = Version.Parse(LauncherAssemblyInfo.FileVersion);
        if (_fileSystem.File.ReadAllBytes(filePath).Length > 0)
        {
            var existingVersion = Version.Parse(FileVersionInfo.GetVersionInfo(filePath).FileVersion);
            if (existingVersion >= currentFileVersion)
                return false;
        }
        return true;
    }

    private async Task WriteToFileAsync(Stream assemblyStream, string filePath)
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