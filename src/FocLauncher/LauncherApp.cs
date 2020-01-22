using System;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher.Properties;
using FocLauncher.Theming;
using FocLauncher.Threading;
using FocLauncher.WaitDialog;

namespace FocLauncher
{
    public class LauncherApp : Application
    {
        static LauncherApp()
        {
            // Since FocLauncher.Threading.dll and Microsoft.VisualStudio.Utilities.dll are used by the WaitWindow AppDomain we need to have them on disk
            // Make sure not to use async file writing as we need to block the app until necessary assembly are written to disk
            AssemblyExtractor.WriteNecessaryAssembliesToDisk(LauncherDataModel.AppDataPath, "FocLauncher.Threading.dll", "Microsoft.VisualStudio.Utilities.dll");
        }

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
            
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var data = new WaitDialogProgressData("Please wait while the launcher is loading an update.", "Updating....", null, true);

                
                var s = WaitDialogFactory.Instance.StartWaitDialog("123", data, TimeSpan.FromSeconds(2));
                try
                {
                    await Task.Delay(50000, s.UserCancellationToken);

                    //foreach (var func in actionQueue) 
                    //    await func();
                }
                catch (TaskCanceledException)
                {
                }
                finally
                {
                    s.Dispose();
                }
            });
        }

        private void LauncherApp_Exit(object sender, ExitEventArgs e)
        {
            Shutdown();
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