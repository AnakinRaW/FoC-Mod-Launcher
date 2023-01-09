using System;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Theming;

public interface IThemeManager
{
    event EventHandler<ThemeChangedEventArgs> ThemeChanged;

    ITheme Theme { get; set; }

    void Initialize(System.Windows.Application app, ITheme? defaultTheme);
}