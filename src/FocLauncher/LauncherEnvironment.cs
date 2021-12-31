using System;
using System.IO.Abstractions;
using System.Threading;
using Validation;

namespace FocLauncher;

internal class LauncherEnvironment : ILauncherEnvironment
{
    public const string ApplicationLocalDirectoryName = "FocLauncher";
    public const string LauncherLogDirectoryName = "FocLauncher_Logs";

    public const string LauncherProgramName = "FoC Launcher";

    private string? _launcherLocalPath;
    private IDirectoryInfo? _localDirectory;
    private readonly IFileSystem _fileSystem;

    public string ApplicationLocalPath => LazyInitializer.EnsureInitialized(ref _launcherLocalPath, BuildLocalPath)!;

    public IDirectoryInfo ApplicationLocalDirectory =>
        _localDirectory ??= _fileSystem.DirectoryInfo.FromDirectoryName(ApplicationLocalPath);

    public LauncherEnvironment(IFileSystem fileSystem)
    {
        Requires.NotNull(fileSystem, nameof(fileSystem));
        _fileSystem = fileSystem;
    }

    private string BuildLocalPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return _fileSystem.Path.Combine(appDataPath, ApplicationLocalDirectoryName);
    }
}