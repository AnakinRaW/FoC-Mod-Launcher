using System.Windows;
using FocLauncher.AssemblyHelper;

namespace FocLauncher
{
    public partial class App
    {
        static App()
        {
            AssemblyLoader.LoadAssemblies();
        }

        private void OnStartUp(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow();
            var viewModel = new MainWindowViewModel(new LauncherDataModel());

            mainWindow.DataContext = viewModel;
            mainWindow.Show();
        }
    }
}
