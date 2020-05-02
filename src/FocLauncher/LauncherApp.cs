using System.Windows;
using FocLauncher.Properties;
using FocLauncher.Theming;
using NLog;

namespace FocLauncher
{
    public class LauncherApp : Application
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Default.Save();
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.Trace("Starting LauncherApp");

            Exit += LauncherApp_Exit;

            base.OnStartup(e);
            var mainWindow = new MainWindow();

            ThemeManager.Initialize(mainWindow);
            LauncherServiceProvider.Instance.RegisterService(ThemeManager.Instance, typeof(IThemeManager));
            ThemeManager.Instance.ApplySavedDefaultTheme();
            
            var viewModel = new MainWindowViewModel();

            mainWindow.DataContext = viewModel;
            mainWindow.Show();
        }

        private void LauncherApp_Exit(object sender, ExitEventArgs e)
        {
            Shutdown();
        }
    }
}