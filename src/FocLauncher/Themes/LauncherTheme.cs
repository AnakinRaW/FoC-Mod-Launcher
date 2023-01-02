using System;
using System.Windows;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Theming;
using Sklavenwalker.CommonUtilities.Wpf.Themes.Fonts;

namespace FocLauncher.Themes;

internal static class LauncherColors
{
    private static ComponentResourceKey? _mainWindowText;

    public static ComponentResourceKey MainWindowText =>
        _mainWindowText ??= new ComponentResourceKey(typeof(LauncherColors), nameof(MainWindowText));
}

internal class LauncherTheme : Theme
{
    public override string Id => "LauncherDefaultTheme";
    public override string Text => "Default Theme";
    public override Uri ResourceUri =>
        new(typeof(LauncherTheme).Assembly.GetName().Name + ";component/Themes/LauncherTheme.xaml", UriKind.Relative);

    public override ResourceDictionary CustomResources()
    {
        var resources = new ResourceDictionary
        {
            { EnvironmentFonts.CaptionFontSizeKey, 18.0 },
            {
                EnvironmentFonts.CaptionFontFamilyKey, new FontFamily(
                    new Uri("pack://application:,,,/FocLauncher;component/Resources/Fonts/", UriKind.Absolute),
                    "./#Empire At War Bold")
            }
        };
        return resources;
    }
}