using System;
using System.Collections.Generic;
using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Theming;

public abstract class Theme : ITheme
{
    public abstract string Id { get; }

    public abstract string Text { get; }
    public abstract Uri ResourceUri { get; }

    public virtual IReadOnlyList<ITheme> SubThemes { get; } = Array.Empty<ITheme>();

    public bool Equals(ITheme? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other == null)
            return false;
        return ResourceUri == other.ResourceUri && Id == other.Id;
    }

    public virtual ResourceDictionary? CustomResources()
    {
        return null;
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

    protected Uri GetThemeUri(string themeFilePath)
    {
        return new(GetType().Assembly.GetName().Name + $";component/{themeFilePath}", UriKind.Relative);
    }
}