using System;
using System.Windows;

namespace FocLauncher.Theming
{
    public interface IThemeManager
    {
        event EventHandler<ThemeChangedEventArgs> ThemeChanged;

        ITheme Theme { get; set; }

        void ApplySavedDefaultTheme();

        void RegisterTheme(ITheme theme);

        uint GetThemedColorRgba(ComponentResourceKey componentResourceKey);
    }
}
