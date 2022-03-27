using System;

namespace Sklavenwalker.ProductMetadata.Component;

public interface IProductComponentIdentity : IEquatable<IProductComponentIdentity>
{
    string Id { get; }

    Version? Version { get; }

    string? Branch { get; }

    string GetUniqueId();
}