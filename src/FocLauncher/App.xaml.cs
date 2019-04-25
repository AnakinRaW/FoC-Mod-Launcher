using System.Windows;
using FocLauncher.AssemblyHelper;
using FocLauncher.Properties;
using FocLauncher.Theming;

namespace FocLauncher
{
    public partial class App
    {
        static App()
        {
            AssemblyLoader.LoadAssemblies();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Default.Save();
            base.OnExit(e);
        }

        private void OnStartUp(object sender, StartupEventArgs e)
        {
            ThemeManager.Initialize();
            var mainWindow = new MainWindow();

            var dataModel = new LauncherDataModel();
            dataModel.Initialized += OnDataModelInitialized;

            dataModel.Initialize();
            var viewModel = new MainWindowViewModel(dataModel);

            mainWindow.DataContext = viewModel;
            mainWindow.Show();
        }

        private static void OnDataModelInitialized(object sender, System.EventArgs e)
        {
            ThemeManager.Instance.ApplySavedDefaultTheme();
        }
    }
}
