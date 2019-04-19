using System.Windows;

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
    }
}
