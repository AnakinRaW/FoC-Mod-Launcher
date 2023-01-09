using System;
using Semver;

namespace AnakinRaW.ProductMetadata.Component;

public interface IProductComponentIdentity : IEquatable<IProductComponentIdentity>
{
    string Id { get; }

    SemVersion? Version { get; }

    string? Branch { get; }

    string GetUniqueId();
}