using System.Windows;

namespace AnakinRaW.AppUpdaterFramework.Themes;

internal static class UpdateColors
{
    public static ComponentResourceKey UpdateProductBackground { get; } = new(typeof(UpdateColors), nameof(UpdateProductBackground));
    public static ComponentResourceKey UpdateProductBorder { get; } = new(typeof(UpdateColors), nameof(UpdateProductBorder));
    public static ComponentResourceKey UpdateInfoBarBackground { get; } = new(typeof(UpdateColors), nameof(UpdateInfoBarBackground));
    public static ComponentResourceKey UpdateInfoBarBorder { get; } = new(typeof(UpdateColors), nameof(UpdateInfoBarBorder));
}