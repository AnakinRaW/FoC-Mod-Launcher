using System;
using Semver;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Component;

public interface IProductComponentIdentity : IEquatable<IProductComponentIdentity>
{
    string Id { get; }

    SemVersion? Version { get; }

    string? Branch { get; }

    string GetUniqueId();
}