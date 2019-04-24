using System;
using FocLauncher.Theming;

namespace RawTheme
{
    public class Theme : AbstractTheme
    {
        public override string Name => "RaW Theme";

        public override Uri GetResourceUri()
        {
            return new Uri("/RawTheme;component/Theme.xaml", UriKind.Relative);
        }
    }
}
