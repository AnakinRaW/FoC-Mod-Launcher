using System;
using FocLauncher.Theming;

namespace FocLauncher.Theme
{
    public class LauncherTheme : ITheme
    {
        public string Name => "Default";

        public Uri GetResourceUri()
        {
            return new Uri("/FocLauncher.Theme;component/LauncherTheme.xaml", UriKind.Relative);
        }
    }
}
