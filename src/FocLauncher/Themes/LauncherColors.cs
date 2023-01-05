using System.Windows;

namespace FocLauncher.Themes;

internal static class LauncherColors
{
    private static ComponentResourceKey? _mainWindowText;

    private static ComponentResourceKey? _aboutWindowPackagesText;

    public static ComponentResourceKey MainWindowText =>
        _mainWindowText ??= new ComponentResourceKey(typeof(LauncherColors), nameof(MainWindowText));

    public static ComponentResourceKey AboutWindowPackagesText =>
        _aboutWindowPackagesText ??= new ComponentResourceKey(typeof(LauncherColors), nameof(AboutWindowPackagesText));
}