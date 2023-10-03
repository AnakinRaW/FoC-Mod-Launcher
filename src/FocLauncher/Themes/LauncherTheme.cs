using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using AnakinRaW.AppUpdaterFramework.Themes;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Controls;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Theming;
using AnakinRaW.CommonUtilities.Wpf.Controls;
using AnakinRaW.CommonUtilities.Wpf.Themes.Fonts;

namespace FocLauncher.Themes;

internal class LauncherTheme : Theme
{
    public override string Id => "LauncherDefaultTheme";
    public override string Text => "Launcher Theme (Dark)";
    public override Uri ResourceUri => GetThemeUri("Themes/LauncherTheme.xaml");

    public override IReadOnlyList<ITheme> SubThemes => new List<ITheme>
    {
        new DarkUpdaterTheme()
    };

    public override ResourceDictionary CustomResources()
    {
        var resources = new ResourceDictionary
        {
            { EnvironmentFonts.CaptionFontSizeKey, 18.0 },
            {
                EnvironmentFonts.CaptionFontFamilyKey, new FontFamily(
                    new Uri($"pack://application:,,,/{GetType().Assembly.GetName().Name};component/Resources/Fonts/", UriKind.Absolute),
                    "./#Empire At War Bold")
            }
        };

        //resources.MergedDictionaries.Add(DeferredStyleResourceDictionary.Instance);

        return resources;
    }
}

public class DeferredStyleResourceDictionary : ResourceDictionary
{
    private static readonly Lazy<ResourceDictionary> LazyInstance =
        new(() => new DeferredStyleResourceDictionary());

    private static readonly IReadOnlyDictionary<object, string> DefaultStyles =
        new Dictionary<object, string>
        {
            {
                StylingContextMenu.ButtonStyleKey,
                "ContextMenuStyle.xaml"
            }
        };
    private static readonly Dictionary<object, object> RealizedStyles = new();

    public DeferredStyleResourceDictionary()
    {
        foreach (var defaultStyle in DefaultStyles)
            Add(defaultStyle.Key, defaultStyle.Key);
    }

    public static ResourceDictionary Instance => LazyInstance.Value;

    protected override void OnGettingValue(object key, ref object value, out bool canCache)
    {
        base.OnGettingValue(key, ref value, out canCache);

        //if (RealizedStyles.TryGetValue(key, out value))
        //    canCache = true;
        //else
        //{
        //    if (DefaultStyles.TryGetValue(key, out var styleName))
        //    {
        //        value = RealizeValue(key, styleName);
        //        canCache = true;
        //    }
        //    else
        //        base.OnGettingValue(key, ref value, out canCache);
        //}

        //static object RealizeValue(object key, string styleName)
        //{
        //    var uri = new Uri("/Microsoft.VisualStudio.Shell.Styles;component/Styles/" + styleName, UriKind.Relative);
        //    var obj = new ResourceDictionary()
        //    {
        //        Source = uri
        //    }[key];
        //    RealizedStyles.Add(key, obj);
        //    return obj;
        //}
    }
}