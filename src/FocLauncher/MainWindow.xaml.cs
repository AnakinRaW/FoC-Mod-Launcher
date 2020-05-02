using System.ComponentModel;
using System.Windows;
using FocLauncher.Dialogs;
using FocLauncher.Mods;

namespace FocLauncher
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ListBox.Focus();
        }

        private void OpenAboutWindow(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        private void OpenSettingsWindow(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().ShowDialog();
        }

        private void OpenChangeThemeDialog(object sender, RoutedEventArgs e)
        {
            new ChangeThemeDialog().ShowDialog();
        }
        
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            SteamModNamePersister.Instance.Save();
        }
    }
}
