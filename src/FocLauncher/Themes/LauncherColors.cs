using System.Windows;

namespace FocLauncher.Themes;

internal static class LauncherColors
{
    public static ComponentResourceKey MainWindowText { get; } = new(typeof(LauncherColors), nameof(MainWindowText));

    public static ComponentResourceKey AboutWindowPackagesText { get; } = new(typeof(LauncherColors), nameof(AboutWindowPackagesText));
}