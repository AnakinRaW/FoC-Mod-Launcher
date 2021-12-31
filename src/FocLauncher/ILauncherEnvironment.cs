using System.IO.Abstractions;

namespace FocLauncher;

internal interface ILauncherEnvironment
{
    public string ApplicationLocalPath { get; }

    public IDirectoryInfo ApplicationLocalDirectory { get; }
}