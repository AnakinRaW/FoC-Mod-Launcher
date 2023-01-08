using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Themes.Colors;

public static class CommandBarColors
{
    public static ComponentResourceKey CommandBarTextActive { get; } = new(typeof(CommandBarColors), nameof(CommandBarTextActive));
    public static ComponentResourceKey CommandBarTextInactive { get; } = new(typeof(CommandBarColors), nameof(CommandBarTextInactive));
    public static ComponentResourceKey CommandBarMenuBackground { get; } = new(typeof(CommandBarColors), nameof(CommandBarMenuBackground));
    public static ComponentResourceKey CommandBarMenuBorder { get; } = new(typeof(CommandBarColors), nameof(CommandBarMenuBorder));
    public static ComponentResourceKey CommandBarMenuIconBackground { get; } = new(typeof(CommandBarColors), nameof(CommandBarMenuIconBackground));
    public static ComponentResourceKey CommandBarMenuItemHover { get; } = new(typeof(CommandBarColors), nameof(CommandBarMenuItemHover));
    public static ComponentResourceKey CommandBarMenuItemHoverBorder { get; } = new(typeof(CommandBarColors), nameof(CommandBarMenuItemHoverBorder));
    public static ComponentResourceKey CommandBarMenuItemHoverText { get; } = new(typeof(CommandBarColors), nameof(CommandBarMenuItemHoverText));
    public static ComponentResourceKey CommandBarMenuItemGlyph { get; } = new(typeof(CommandBarColors), nameof(CommandBarMenuItemGlyph));
    public static ComponentResourceKey CommandBarMenuItemHoverGlyph { get; } = new(typeof(CommandBarColors), nameof(CommandBarMenuItemHoverGlyph));
    public static ComponentResourceKey CommandBarMenuScrollGlyph { get; } = new(typeof(CommandBarColors), nameof(CommandBarMenuScrollGlyph));
    public static ComponentResourceKey CommandBarMenuSeparatorBrush { get; } = new(typeof(CommandBarColors), nameof(CommandBarMenuSeparatorBrush));
    public static ComponentResourceKey DropShadowBackgroundColor { get; } = new(typeof(CommandBarColors), nameof(DropShadowBackgroundColor));
}