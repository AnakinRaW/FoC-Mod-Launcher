using System;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging;

public struct ImageMoniker
{
    public Type CatalogType;
    public string Name;
    
    public static bool operator ==(ImageMoniker moniker1, ImageMoniker moniker2)
    {
        return moniker1.Equals(moniker2);
    }

    public static bool operator !=(ImageMoniker moniker1, ImageMoniker moniker2)
    {
        return !moniker1.Equals(moniker2);
    }

    public override bool Equals(object obj)
    {
        return obj is ImageMoniker other && Equals(other);
    }

    public bool Equals(ImageMoniker other)
    {
        return CatalogType == other.CatalogType && Name == other.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CatalogType, Name);
    }
}