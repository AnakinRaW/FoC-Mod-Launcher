using System.IO.Abstractions;
using Flurl;
using Semver;

namespace AnakinRaW.ApplicationBase;

public interface IApplicationEnvironment
{
    SemVersion InformationalVersion { get; }

    string ApplicationName { get; }

    string ApplicationLocalPath { get; }

    IDirectoryInfo ApplicationLocalDirectory { get; }

    public Url? RepositoryUrl { get; }
}