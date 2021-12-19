using System;
using System.Globalization;
using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Converters;

public class WindowStateConverter : ValueConverter<int, WindowState>
{
    protected override WindowState Convert(int state, object? parameter, CultureInfo culture)
    {
        return state switch
        {
            1 => WindowState.Normal,
            2 => WindowState.Minimized,
            3 => WindowState.Maximized,
            _ => throw new ArgumentException("State must be one of SW_SHOWNORMAL, SW_SHOWMINIMIZED, or SW_SHOWMAXIMIZED.")
        };
    }

    protected override int ConvertBack(WindowState state, object? parameter, CultureInfo culture)
    {
        return state switch
        {
            WindowState.Normal => 1,
            WindowState.Minimized => 2,
            WindowState.Maximized => 3,
            _ => throw new ArgumentException("State must be one of Normal, Minimized, or Maximized.")
        };
    }
}