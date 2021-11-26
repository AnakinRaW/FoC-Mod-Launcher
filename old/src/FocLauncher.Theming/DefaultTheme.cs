using System;

namespace FocLauncher.Theming
{
    public class DefaultTheme : AbstractTheme
    {
        public override string Name => "Default";

        public override Uri GetResourceUri()
        {
            return new Uri("/FocLauncher.Theming;component/DefaultTheme.xaml", UriKind.Relative);
        }
    }
}
