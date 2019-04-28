using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocLauncherApp.Updater;
using FocLauncherApp.WaitDialog;

namespace FocLauncherApp
{
    public class BootstrapperApp : Application
    {
        public const string ServerUrl = "https://raw.githubusercontent.com/AnakinSklavenwalker/FoC-Mod-Launcher/master";
        public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"FoC Launcher\");

        protected override async void OnStartup(StartupEventArgs e)
        {
            var updater = new LauncherUpdater();
            var result = NativeMethods.NativeMethods.InternetGetConnectedState(out var flags, 0);

            await Dispatcher.Invoke(Update);
            base.OnStartup(e);


            Shutdown(0);
        }

        private static async Task Update()
        {
            var wd = WaitDialogFactory.CreateInstance();
            var cancellationTokenSource = new CancellationTokenSource();
            wd.StartWaitDialog("FoC Launcher", "Please wait while the launcher is loading an update.", "Updating....", 2, false, true);
            await Task.Run(() => AsyncMethod2(cancellationTokenSource.Token), cancellationTokenSource.Token);
            wd.EndWaitDialog(out _);
        }


        private static async Task AsyncMethod2(CancellationToken token)
        {
            try
            {
                await Task.Delay(1000, token);
                await Task.Delay(1000, token);
                await Task.Delay(1000, token);
                await Task.Delay(1000, token);
                await Task.Delay(1000, token);
                MessageBox.Show("Completed");
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}