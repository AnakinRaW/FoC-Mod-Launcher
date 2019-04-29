using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FocLauncherApp.WaitDialog
{
    internal static class WaitDialogHelper
    {
        internal static async Task RunWithWaitDialog(Func<CancellationToken, Task> action, string caption, string waitMessage,
            string progressText, int delayToShowWindow, bool isCancelable, bool showMarqueeProgress)
        {
            await Application.Current.Dispatcher.Invoke(async () =>
            {
                var wd = WaitDialogFactory.CreateInstance();
                var cancellationTokenSource = new CancellationTokenSource();
                try
                {
                    wd.StartWaitDialog(caption, waitMessage, progressText, delayToShowWindow, isCancelable, showMarqueeProgress);
                    await Task.Run(() => action(cancellationTokenSource.Token), cancellationTokenSource.Token);
                }
                finally
                {
                    wd.EndWaitDialog(out _);
                }
            });
        }

        internal static async Task RunWithWaitDialog(Func<Task> action, string caption, string waitMessage,
            string progressText, int delayToShowWindow, bool showMarqueeProgress)
        {
            await RunWithWaitDialog(token => action(), caption, waitMessage, progressText, delayToShowWindow, false, showMarqueeProgress);
        }
    }
}
