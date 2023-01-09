namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Theming;

internal interface IThemeResourceDictionaryCache
{
    ThemeResourceDictionary? Get(ITheme theme);

    ThemeResourceDictionary GetOrCreate(ITheme theme);
}