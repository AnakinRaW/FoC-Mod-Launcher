namespace Sklavenwalker.CommonUtilities.Wpf.Theming;

internal interface IThemeResourceDictionaryCache
{
    ThemeResourceDictionary? Get(ITheme theme);

    ThemeResourceDictionary GetOrCreate(ITheme theme);
}