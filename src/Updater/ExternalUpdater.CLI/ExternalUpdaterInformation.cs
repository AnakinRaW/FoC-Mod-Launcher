using System;
using System.IO;
using AnakinRaW.ExternalUpdater.Utilities;
using Semver;

namespace AnakinRaW.ExternalUpdater;

public record ExternalUpdaterInformation
{
    internal ExternalUpdaterInformation()
    {
    }

    public required string Name { get; init; }

    public required Version FileVersion { get; init; }

    public required SemVersion InformationalVersion { get; init; }

    public static ExternalUpdaterInformation FromAssemblyStream(Stream assemblyStream)
    {
        if (!ExternalUpdaterUtilities.IsValidAssembly(assemblyStream, out var assemblyInformation))
            throw new BadImageFormatException("The given stream does not represent the external updater binary.");

        return assemblyInformation!;
    }
}