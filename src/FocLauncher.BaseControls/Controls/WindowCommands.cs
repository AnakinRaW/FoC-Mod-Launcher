using System.Windows;
using System.Windows.Input;

namespace FocLauncher.Controls
{
    public class WindowCommands
    {
        public static RoutedCommand MinimizeWindow = new RoutedCommand(nameof(MinimizeWindow), typeof(WindowCommands));
        public static RoutedCommand CloseWindow = new RoutedCommand(nameof(CloseWindow), typeof(WindowCommands));

        static WindowCommands()
        {
            CommandManager.RegisterClassCommandBinding(typeof(UIElement), new CommandBinding(MinimizeWindow, OnMinimizeWindow));
            CommandManager.RegisterClassCommandBinding(typeof(UIElement), new CommandBinding(CloseWindow, OnCloseWindow));
        }

        private static bool CanMinimizeWindow(ExecutedRoutedEventArgs args)
        {
            return args.Parameter is Window;
        }

        private static void OnMinimizeWindow(object sender, ExecutedRoutedEventArgs args)
        {
            if (!CanMinimizeWindow(args))
                return;
            ((Window)args.Parameter).WindowState = WindowState.Minimized;
        }

        private static bool CanCloseWindow(ExecutedRoutedEventArgs args)
        {
            return args.Parameter is Window;
        }

        private static void OnCloseWindow(object sender, ExecutedRoutedEventArgs args)
        {
            if (!CanCloseWindow(args))
                return;
            ((Window)args.Parameter).Close();
        }
    }
}
