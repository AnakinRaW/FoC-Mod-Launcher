using System;
using Sklavenwalker.CommonUtilities.Wpf.Theming;

namespace FocLauncher.Themes
{
    internal class LauncherTheme : Theme
    {
        public override string Id => "LauncherDefaultTheme";
        public override string Text => "Default Theme";
        public override Uri ResourceUri =>
            new(typeof(LauncherTheme).Assembly.GetName().Name + ";component/Themes/LauncherTheme.xaml", UriKind.Relative);
    }
}
