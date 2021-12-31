using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Theming;

internal class ThemeResourceDictionaryCache : IThemeResourceDictionaryCache
{
    private readonly IThemeResourceDictionaryBuilder _builder;

    private readonly Dictionary<ITheme, ThemeResourceDictionary> _themeResourceCache = new();

    public ThemeResourceDictionaryCache(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _builder = serviceProvider.GetRequiredService<IThemeResourceDictionaryBuilder>();
    }

    public ThemeResourceDictionary? Get(ITheme theme)
    {
        _themeResourceCache.TryGetValue(theme, out var resourcesDictionary);
        return resourcesDictionary;
    }

    public ThemeResourceDictionary GetOrCreate(ITheme theme)
    {
        if (_themeResourceCache.TryGetValue(theme, out var resourcesDictionary))
            return resourcesDictionary;
        resourcesDictionary = _builder.BuildTheme(theme);
        _themeResourceCache[theme] = resourcesDictionary;
        return resourcesDictionary;
    }
}