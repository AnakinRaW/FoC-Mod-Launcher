using System;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Theming;

public class DefaultTheme : Theme
{
    public override string Id => "DefaultTheme";
    public override string Text => "(Default Theme)";

    public override Uri ResourceUri =>
        new(typeof(DefaultTheme).Assembly.GetName().Name + ";component/Themes/DefaultTheme.xaml", UriKind.Relative);
}