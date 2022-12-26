using System;
using System.Collections.ObjectModel;
using System.Windows;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Theming;

internal class ThemeResourceDictionary : ResourceDictionary, IEquatable<ThemeResourceDictionary>
{
    private readonly ITheme _theme;

    public ThemeResourceDictionary(ITheme theme, Collection<ResourceDictionary> resources)
    {
        Requires.NotNull(theme, nameof(theme));
        Requires.NotNull(resources, nameof(resources));
        _theme = theme;
        foreach (var dictionary in resources) 
            MergedDictionaries.Add(dictionary);
    }

    public bool Equals(ThemeResourceDictionary? other)
    {
        if (other is null)
            return false;
        return ReferenceEquals(this, other) || _theme.Equals(other._theme);
    }

    public override bool Equals(object? obj)
    {
        return obj is ThemeResourceDictionary other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _theme.GetHashCode();
    }
}