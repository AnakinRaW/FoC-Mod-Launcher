using System;

namespace AnakinRaW.AppUpdaterFramework.Themes;

public class UpdateTheme
{ 
    public static Uri ResourceUri =>
        new(typeof(UpdateTheme).Assembly.GetName().Name + ";component/Themes/UpdateTheme.xaml", UriKind.Relative);
}