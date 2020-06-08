using System;
using System.IO;
using System.Windows;
using FocLauncher.Properties;
using FocLauncher.Theming;
using NLog;
using static FocLauncher.Properties.Resources;

namespace FocLauncher
{
    public class LauncherApp : Application
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string IconPath = Path.Combine(LauncherConstants.ApplicationBasePath, "foc.ico");

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Default.Save();
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += LauncherApp_DispatcherUnhandledException;

            Logger.Trace("Starting LauncherApp");
            Exit += LauncherApp_Exit;
            base.OnStartup(e);
            InstallFocIcon();

            ApplicationResourceLoader.LoadResources();

            var mainWindow = new MainWindow();

            ThemeManager.Initialize(mainWindow);
            LauncherServiceProvider.Instance.RegisterService(ThemeManager.Instance, typeof(IThemeManager));
            ThemeManager.Instance.ApplySavedDefaultTheme();

            var viewModel = new MainWindowViewModel(mainWindow);

            mainWindow.DataContext = viewModel;
            mainWindow.Show();
        }
        
        private static void LauncherApp_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is LauncherException)
                return;
            if (Attribute.GetCustomAttribute(e.Exception.GetType(),
                    typeof(SerializableAttribute)) == null)
                throw new LauncherException(e.Exception.Message);
        }

        private static void InstallFocIcon()
        {
            if (File.Exists(IconPath))
                return;
            using var fs = new FileStream(IconPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            foc.Save(fs);
        }

        private void LauncherApp_Exit(object sender, ExitEventArgs e)
        {
            Shutdown();
        }
    }
}