using System;

namespace AnakinRaW.AppUpaterFramework.Metadata.Component;

public sealed record OriginInfo(Uri Url)
{
    public long? Size { get; init; }

    public ComponentIntegrityInformation IntegrityInformation { get; init; }
}