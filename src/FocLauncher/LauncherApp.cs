using System.Windows;
using FocLauncher.Core.Properties;
using FocLauncher.Core.Theming;

namespace FocLauncher.Core
{
    public class LauncherApp : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Default.Save();
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Exit += LauncherApp_Exit;
            DispatcherUnhandledException += WrapException;

            base.OnStartup(e);
            var mainWindow = new MainWindow();

            ThemeManager.Initialize(mainWindow);


            var dataModel = new LauncherDataModel();
            dataModel.Initialized += OnDataModelInitialized;

            dataModel.Initialize();

            var viewModel = new MainWindowViewModel(dataModel);

            mainWindow.DataContext = viewModel;
            mainWindow.Show();
        }

        private void LauncherApp_Exit(object sender, ExitEventArgs e)
        {
            MainWindow?.Dispatcher.InvokeShutdown();
            Dispatcher.InvokeShutdown();       
        }

        private static void WrapException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is LauncherException)
                return;

            var message = $"{e.Exception.Message} ({e.Exception.GetType().Name})";
            var wrappedException = new LauncherException(message, e.Exception);
            wrappedException.Data.Add(LauncherException.LauncherDataModelKey, LauncherDataModel.Instance.GetDebugInfo());
            throw wrappedException;
        }

        private static void OnDataModelInitialized(object sender, System.EventArgs e)
        {
            ThemeManager.Instance.ApplySavedDefaultTheme();
        }
    }
}