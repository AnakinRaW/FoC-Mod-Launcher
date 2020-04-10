using System;

namespace FocLauncher.Theming
{
    public sealed class ThemeChangedEventArgs : EventArgs
    {
        public ITheme Theme { get; }

        public ThemeChangedEventArgs(ITheme theme)
        {
            Theme = theme;
        }
    }
}