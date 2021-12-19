using System;

namespace Sklavenwalker.CommonUtilities.Wpf.Theming;

public class FallbackTheme : Theme
{
    public override string Id => "FallbackTheme";
    public override string Text => "(Fallback Theme)";

    public override Uri ResourceUri =>
        new(typeof(FallbackTheme).Assembly.GetName().Name + ";component/Themes/Colors/FallbackTheme.xaml", UriKind.Relative);
}