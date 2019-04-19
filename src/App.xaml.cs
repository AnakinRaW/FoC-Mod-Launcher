using System.Windows;

namespace FocLauncher
{
    public partial class App
    {
        private void OnStartUp(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow();
            var viewModel = new MainWindowViewModel(new LauncherDataModel());

            mainWindow.DataContext = viewModel;
            mainWindow.Show();
        }
    }
}
