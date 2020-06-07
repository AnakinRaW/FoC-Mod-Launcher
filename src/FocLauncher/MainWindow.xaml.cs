using System.ComponentModel;
using System.Windows;
using FocLauncher.Dialogs;
using FocLauncher.Mods;
using FocLauncher.Theming;

namespace FocLauncher
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ScrollBarThemingUtilities.SetThemeScrollBars(this, true);
            ListBox.Focus();
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
