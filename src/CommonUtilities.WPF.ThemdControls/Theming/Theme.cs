using System;

namespace Sklavenwalker.CommonUtilities.Wpf.Theming;

public abstract class Theme : ITheme
{
    public abstract string Id { get; }

    public abstract string Text { get; }
    public abstract Uri ResourceUri { get; }
    
    public bool Equals(ITheme? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other == null)
            return false;
        return ResourceUri == other.ResourceUri && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;
        return obj is ITheme other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, ResourceUri);
    }
}