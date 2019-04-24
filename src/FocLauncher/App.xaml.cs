using System.Windows;
using FocLauncher.AssemblyHelper;
using FocLauncher.Theming;

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
            ThemeManager.Initialize();
            var mainWindow = new MainWindow();
            var viewModel = new MainWindowViewModel(new LauncherDataModel());

            mainWindow.DataContext = viewModel;
            mainWindow.Show();
        }
    }
}
