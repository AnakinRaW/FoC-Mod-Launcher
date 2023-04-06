using System;
using System.IO.Abstractions;
using System.Reflection;
using System.Threading;
using Flurl;
using Validation;

namespace AnakinRaW.ApplicationBase;

public abstract class ApplicationEnvironmentBase : IApplicationEnvironment
{
    private string? _launcherLocalPath;
    private IDirectoryInfo? _localDirectory;
    private readonly IFileSystem _fileSystem;

    public abstract string ApplicationName { get; }
    public abstract Url? RepositoryUrl { get; }
    public abstract Url UpdateRootUrl { get; }
    public abstract string ApplicationRegistryPath { get; }
    public string ApplicationLocalPath => LazyInitializer.EnsureInitialized(ref _launcherLocalPath, BuildLocalPath)!;
    public IDirectoryInfo ApplicationLocalDirectory =>
        _localDirectory ??= _fileSystem.DirectoryInfo.New(ApplicationLocalPath);

    public ApplicationAssemblyInfo AssemblyInfo { get; }

    protected abstract string ApplicationLocalDirectoryName { get; }

    protected ApplicationEnvironmentBase(Assembly assembly, IFileSystem fileSystem)
    {
        Requires.NotNull(assembly, nameof(assembly));
        Requires.NotNull(fileSystem, nameof(fileSystem));
        _fileSystem = fileSystem;
        AssemblyInfo = new ApplicationAssemblyInfo(assembly);
    }

    private string BuildLocalPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return _fileSystem.Path.Combine(appDataPath, ApplicationLocalDirectoryName);
    }
}