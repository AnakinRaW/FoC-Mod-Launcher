using System;

namespace Sklavenwalker.CommonUtilities.Wpf.Theming;

/// <summary>
/// The event args when <see cref="IThemeManager.ThemeChanged"/> was raised.
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
    /// <summary>
    /// The new theme.
    /// </summary>
    public ITheme NewTheme { get; }

    /// <summary>
    /// The old theme.
    /// </summary>
    public ITheme OldTheme { get; }

    public ThemeChangedEventArgs(ITheme newTheme, ITheme oldTheme)
    {
        NewTheme = newTheme;
        OldTheme = oldTheme;
    }
}