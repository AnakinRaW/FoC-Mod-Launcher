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
        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Default.Save();
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Exit += LauncherApp_Exit;

            base.OnStartup(e);
            var mainWindow = new MainWindow();

            ThemeManager.Initialize(mainWindow);

            var dataModel = new LauncherDataModel();
            dataModel.Initialized += OnDataModelInitialized;

            dataModel.Initialize();

            var viewModel = new MainWindowViewModel(dataModel);

            mainWindow.DataContext = viewModel;
            mainWindow.Show();

            //ThreadHelper.JoinableTaskFactory.Run(async () =>
            //{
            //    var data = new WaitDialogProgressData("Please wait while the launcher is loading an update.", "Updating....", null, true);


            //    var s = WaitDialogFactory.Instance.StartWaitDialog("123", data, TimeSpan.FromSeconds(2));
            //    try
            //    {
            //        await Task.Delay(50000, s.UserCancellationToken);

            //        //foreach (var func in actionQueue) 
            //        //    await func();
            //    }
            //    catch (TaskCanceledException)
            //    {
            //    }
            //    finally
            //    {
            //        s.Dispose();
            //    }
            //});
        }

        private void LauncherApp_Exit(object sender, ExitEventArgs e)
        {
            Shutdown();
        }

        private static void OnDataModelInitialized(object sender, EventArgs e)
        {
            ThemeManager.Instance.ApplySavedDefaultTheme();
        }
    }
}