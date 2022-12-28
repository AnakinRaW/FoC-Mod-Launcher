using System;
using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Theming;

internal class ThemeResourceDictionaryBuilder : IThemeResourceDictionaryBuilder
{
    public ThemeResourceDictionary BuildTheme(ITheme theme)
    {
        var themeResources = new ResourceDictionary();

        var colorResources = LoadResourcesFromUri(theme.ResourceUri);
        if (colorResources is not null)
            themeResources.MergedDictionaries.Add(colorResources);

        var customResources = theme.CustomResources();
        if (customResources is not null)
            themeResources.MergedDictionaries.Add(customResources);

        return new ThemeResourceDictionary(theme, themeResources.MergedDictionaries);
    }

    private static ResourceDictionary? LoadResourcesFromUri(Uri uri)
    {
        try
        {
            return System.Windows.Application.LoadComponent(uri) as ResourceDictionary;
        }
        catch
        {
            return null;
        }
    }
}