using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using FocLauncher.Dialogs;
using FocLauncher.Mods;
using FocLauncher.Theming;
using FocLauncher.Utilities;
using Microsoft.VisualStudio.Threading;

namespace FocLauncher
{
    public partial class MainWindow
    {
        static MainWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainWindow), new FrameworkPropertyMetadata(typeof(MainWindow)));
            RuntimeHelpers.RunClassConstructor(typeof(ScrollBarThemingUtilities).TypeHandle);
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
    }
}
