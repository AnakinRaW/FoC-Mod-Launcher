using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocLauncherApp.Updater;
using FocLauncherApp.WaitDialog;

namespace FocLauncherApp
{
    public class BootstrapperApp : Application
    {
        public const string ServerUrl = "https://gitlab.com/Republic-at-War/Republic-At-War/raw/";

        protected override void OnStartup(StartupEventArgs e)
        {
            var result = NativeMethods.NativeMethods.InternetGetConnectedState(out var flags, 0);

            Current.Dispatcher.InvokeAsync(Update);
            base.OnStartup(e);
        }

        private static async Task Update()
        {
           

            var wd = WaitDialogFactory.CreateInstance();
            var cancellationTokenSource = new CancellationTokenSource();
            wd.StartWaitDialog("test", "test123", "456", 2, false, true);
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