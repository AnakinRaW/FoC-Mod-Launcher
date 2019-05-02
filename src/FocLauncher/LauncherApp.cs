using System.Windows;
using FocLauncher.Core.AssemblyHelper;
using FocLauncher.Core.Properties;
using FocLauncher.Core.Theming;

namespace FocLauncher.Core
{
    public class LauncherApp : Application
    {
        static LauncherApp()
        {
            AssemblyLoader.LoadEmbeddedAssemblies();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Default.Save();
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += WrapException;

            base.OnStartup(e);
            ThemeManager.Initialize();
            var mainWindow = new MainWindow();

            var dataModel = new LauncherDataModel();
            dataModel.Initialized += OnDataModelInitialized;

            dataModel.Initialize();

            object i = null;
            i.ToString();

            var viewModel = new MainWindowViewModel(dataModel);

            mainWindow.DataContext = viewModel;
            mainWindow.Show();
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