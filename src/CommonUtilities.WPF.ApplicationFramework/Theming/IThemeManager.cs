using System;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Theming;

public interface IThemeManager
{
    event EventHandler<ThemeChangedEventArgs> ThemeChanged;

    ITheme Theme { get; set; }

    void Initialize(System.Windows.Application app, ITheme? defaultTheme);
}