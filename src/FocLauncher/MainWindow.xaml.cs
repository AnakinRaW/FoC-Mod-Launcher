using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FocLauncher.Dialogs;
using FocLauncher.Mods;
using FocLauncher.Theming;
using FocLauncher.Utilities;
using Microsoft.VisualStudio.Threading;

namespace FocLauncher
{
    public partial class MainWindow
    {
        public static RoutedCommand MinimizeWindow = new RoutedCommand(nameof(MinimizeWindow), typeof(MainWindow));
        public static RoutedCommand CloseWindow = new RoutedCommand(nameof(CloseWindow), typeof(MainWindow));


        static MainWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainWindow), new FrameworkPropertyMetadata(typeof(MainWindow)));
            RuntimeHelpers.RunClassConstructor(typeof(ScrollBarThemingUtilities).TypeHandle);
            CommandManager.RegisterClassCommandBinding(typeof(UIElement), new CommandBinding(MinimizeWindow, OnMinimizeWindow));
            CommandManager.RegisterClassCommandBinding(typeof(UIElement), new CommandBinding(CloseWindow, OnCloseWindow));
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            SetWindowIcon();
        }

        private void SetWindowIcon()
        {
            IconHelper.UseWindowIconAsync(windowIcon => Icon = windowIcon).Forget();
        }

        private void OpenAboutWindow(object sender, RoutedEventArgs e)
        {
            new AboutWindow(this).ShowDialog();
        }

        private void OpenSettingsWindow(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(this).ShowDialog();
        }

        private void OpenChangeThemeDialog(object sender, RoutedEventArgs e)
        {
            new ChangeThemeDialog(this).ShowDialog();
        }
        
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            SteamModNamePersister.Instance.Save();
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
