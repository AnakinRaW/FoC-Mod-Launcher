﻿using System;
using System.Windows;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.Themes.Fonts;
using Sklavenwalker.CommonUtilities.Wpf.Theming;

namespace FocLauncher.Themes;

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