using System.Windows.Input;

namespace Sklavenwalker.CommonUtilities.Wpf.Input;

public static class ViewCommands
{
    public static RoutedCommand ToggleMaximizeRestoreWindow = new(nameof(ToggleMaximizeRestoreWindow), typeof(ViewCommands));
    public static RoutedCommand MinimizeWindow = new(nameof(MinimizeWindow), typeof(ViewCommands));
    public static RoutedCommand CloseWindow = new(nameof(CloseWindow), typeof(ViewCommands));
}