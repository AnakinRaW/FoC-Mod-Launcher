using System.Threading;

namespace FocLauncherApp.WaitDialog
{
    public interface IWaitDialog
    {
        void EndWaitDialog(out bool canceled);

        void StartWaitDialog(string caption, string waitMessage, string progressText, bool isCancelable, 
            int delayToShowDialog, bool showProgress, CancellationTokenSource cts);
    }
}