using System.IO.Abstractions;
using Flurl;

namespace AnakinRaW.ApplicationBase;

public interface IApplicationEnvironment
{
    ApplicationAssemblyInfo AssemblyInfo { get; }
    
    string ApplicationName { get; }

    string ApplicationLocalPath { get; }

    IDirectoryInfo ApplicationLocalDirectory { get; }

    Url? RepositoryUrl { get; }

    Url UpdateRootUrl { get; }

    string ApplicationRegistryPath { get; }
}