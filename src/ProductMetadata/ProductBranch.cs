using System;

namespace Sklavenwalker.ProductMetadata;

public class ProductBranch : IEquatable<ProductBranch>
{
    public string Name { get; }

    public Uri ManifestLocation { get; }

    public bool IsPrerelease { get; }

    public ProductBranch(string name, Uri manifestLocation, bool isPrerelease)
    {
        Name = name;
        ManifestLocation = manifestLocation;
        IsPrerelease = isPrerelease;
    }

    public override string ToString()
    {
        return Name;
    }

    public bool Equals(ProductBranch? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ProductBranch)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}