using System;
using System.Collections.Generic;
using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Theming;

internal class ThemeResourceDictionaryBuilder : IThemeResourceDictionaryBuilder
{
    public ThemeResourceDictionary BuildTheme(ITheme theme)
    {
        return BuildTheme(theme, new HashSet<ITheme> { theme });
    }


    private ThemeResourceDictionary BuildTheme(ITheme theme, ISet<ITheme> visitedThemes)
    {
        var themeResources = new ResourceDictionary();

        var colorResources = LoadResourcesFromUri(theme.ResourceUri);
        if (colorResources is not null)
            themeResources.MergedDictionaries.Add(colorResources);

        var subThemes = GetAllSubThemes(theme, visitedThemes);
        foreach (var subTheme in subThemes)
            themeResources.MergedDictionaries.Add(subTheme);

        var customResources = theme.CustomResources();
        if (customResources is not null)
            themeResources.MergedDictionaries.Add(customResources);

        return new ThemeResourceDictionary(theme, themeResources.MergedDictionaries);
    }


    private IEnumerable<ResourceDictionary> GetAllSubThemes(ITheme theme, ISet<ITheme> visitedThemes)
    {
        var themes = new List<ResourceDictionary>();
        foreach (var subTheme in theme.SubThemes)
        {
            if (visitedThemes.Add(subTheme))
                themes.Add(BuildTheme(subTheme, visitedThemes));
        }
        return themes;
    }

    private static ResourceDictionary? LoadResourcesFromUri(Uri uri)
    {
        try
        {
            return Application.LoadComponent(uri) as ResourceDictionary;
        }
        catch
        {
            return null;
        }
    }
}