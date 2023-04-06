using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using AnakinRaW.AppUpdaterFramework.Themes;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Theming;
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
        return resources;
    }
}