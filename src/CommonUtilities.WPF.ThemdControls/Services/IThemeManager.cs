using System;
using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.Theming;

namespace Sklavenwalker.CommonUtilities.Wpf.Services;

public interface IThemeManager
{
    event EventHandler<ThemeChangedEventArgs> ThemeChanged;

    ITheme Theme { get; set; }

    void Initialize(Application app, ITheme? defaultTheme);
}