using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Theming;

namespace AnakinRaW.AppUpdaterFramework.Themes;

public class DarkUpdaterTheme : Theme
{ 
    public override string Id => "UpdaterDarkTheme";

    public override string Text => "Updater Theme (Dark)";

    public override Uri ResourceUri => GetThemeUri("Themes/DarkTheme.xaml");
}