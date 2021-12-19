using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Themes.Colors;

public static class CommandBarColors
{
    private static ComponentResourceKey? _commandBarMenuIconBackground;

    public static ComponentResourceKey CommandBarMenuIconBackground => _commandBarMenuIconBackground ??=
        new ComponentResourceKey(typeof(CommandBarColors), nameof(CommandBarMenuIconBackground));
}