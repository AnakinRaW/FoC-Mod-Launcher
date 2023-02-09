using System;
using Semver;

namespace AnakinRaW.AppUpaterFramework.Metadata.Component;

public interface IProductComponentIdentity : IEquatable<IProductComponentIdentity>
{
    string Id { get; }

    SemVersion? Version { get; }

    string? Branch { get; }

    string GetUniqueId();
}