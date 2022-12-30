using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Themes.Colors;

public static class CommandBarColors
{
    private static ComponentResourceKey? _commandBarTextActive;
    private static ComponentResourceKey? _commandBarTextInactive;
    private static ComponentResourceKey? _commandBarMenuBackground;
    private static ComponentResourceKey? _commandBarMenuBorder;
    private static ComponentResourceKey? _commandBarMenuIconBackground;
    private static ComponentResourceKey? _commandBarMenuItemHover;
    private static ComponentResourceKey? _commandBarMenuItemHoverBorder;
    private static ComponentResourceKey? _commandBarMenuItemHoverText;

    private static ComponentResourceKey? _commandBarMenuItemGlyph;
    private static ComponentResourceKey? _commandBarMenuItemHoverGlyph;
    private static ComponentResourceKey? _commandBarMenuScrollGlyph;

    private static ComponentResourceKey? _commandBarMenuSeparatorBrush;
    private static ComponentResourceKey? _dropShadowBackgroundColor;

    public static ComponentResourceKey CommandBarTextActive => _commandBarTextActive ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(CommandBarTextActive));

    public static ComponentResourceKey CommandBarTextInactive => _commandBarTextInactive ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(CommandBarTextInactive));

    public static ComponentResourceKey CommandBarMenuBackground => _commandBarMenuBackground ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(CommandBarMenuBackground));

    public static ComponentResourceKey CommandBarMenuBorder => _commandBarMenuBorder ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(CommandBarMenuBorder));

    public static ComponentResourceKey CommandBarMenuIconBackground => _commandBarMenuIconBackground ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(CommandBarMenuIconBackground));

    public static ComponentResourceKey CommandBarMenuItemHover => _commandBarMenuItemHover ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(CommandBarMenuItemHover));

    public static ComponentResourceKey CommandBarMenuItemHoverBorder => _commandBarMenuItemHoverBorder ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(CommandBarMenuItemHoverBorder));

    public static ComponentResourceKey CommandBarMenuItemHoverText => _commandBarMenuItemHoverText ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(CommandBarMenuItemHoverText));

    public static ComponentResourceKey CommandBarMenuItemGlyph => _commandBarMenuItemGlyph ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(CommandBarMenuItemGlyph));

    public static ComponentResourceKey CommandBarMenuItemHoverGlyph => _commandBarMenuItemHoverGlyph ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(CommandBarMenuItemHoverGlyph));

    public static ComponentResourceKey CommandBarMenuScrollGlyph => _commandBarMenuScrollGlyph ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(CommandBarMenuScrollGlyph));

    public static ComponentResourceKey CommandBarMenuSeparatorBrush => _commandBarMenuSeparatorBrush ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(CommandBarMenuSeparatorBrush));

    public static ComponentResourceKey DropShadowBackgroundColor => _dropShadowBackgroundColor ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(DropShadowBackgroundColor));
}