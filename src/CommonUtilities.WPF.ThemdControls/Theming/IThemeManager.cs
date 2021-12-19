using System;
using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Theming;

public interface IThemeManager
{
    event EventHandler<ThemeChangedEventArgs> ThemeChanged;

    ITheme Theme { get; set; }

    void Initialize(Application app, Window mainWindow, ITheme? defaultTheme);
}