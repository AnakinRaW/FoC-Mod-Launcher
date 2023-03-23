using System;
using System.IO;
using AnakinRaW.ExternalUpdater.CLI.Utilities;
using Semver;

namespace AnakinRaW.ExternalUpdater.CLI;

public record ExternalUpdaterAssemblyInformation
{
    internal ExternalUpdaterAssemblyInformation()
    {
    }

    public required string Name { get; init; }

    public required Version FileVersion { get; init; }

    public required SemVersion InformationalVersion { get; init; }

    public static ExternalUpdaterAssemblyInformation FromAssemblyStream(Stream assemblyStream)
    {
        if (!ExternalUpdaterUtilities.IsValidAssembly(assemblyStream, out var assemblyInformation))
            throw new BadImageFormatException("The given stream does not represent the external updater binary.");

        return assemblyInformation!;
    }
}