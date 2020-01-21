using System.Threading;

namespace FocLauncherApp.WaitDialog
{
    public interface IWaitDialog
    {
        /// <summary>
        /// Ends the wait dialog.
        /// </summary>
        /// <param name="canceled">if set to <see langword="true"/> the task was canceled by the user.</param>
        void EndWaitDialog(out bool canceled);

        void StartWaitDialog(string caption, string waitMessage, string progressText, bool isCancelable, 
            int delayToShowDialog, bool showProgress, CancellationTokenSource cts);

    }
}