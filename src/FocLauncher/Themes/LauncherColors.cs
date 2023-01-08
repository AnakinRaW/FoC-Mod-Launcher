using System.Windows;

namespace FocLauncher.Themes;

internal static class LauncherColors
{
    public static ComponentResourceKey MainWindowText { get; } = new(typeof(LauncherColors), nameof(MainWindowText));

    public static ComponentResourceKey AboutWindowPackagesText { get; } = new(typeof(LauncherColors), nameof(AboutWindowPackagesText));


    public static ComponentResourceKey UpdateProductBackground { get; } = new(typeof(LauncherColors), nameof(UpdateProductBackground));
    public static ComponentResourceKey UpdateProductBorder { get; } = new(typeof(LauncherColors), nameof(UpdateProductBorder));
    public static ComponentResourceKey UpdateInfoBarBackground { get; } = new(typeof(LauncherColors), nameof(UpdateInfoBarBackground));
    public static ComponentResourceKey UpdateInfoBarBorder { get; } = new(typeof(LauncherColors), nameof(UpdateInfoBarBorder));
}