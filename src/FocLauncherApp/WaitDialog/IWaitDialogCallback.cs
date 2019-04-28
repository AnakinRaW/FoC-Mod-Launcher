namespace FocLauncherApp.WaitDialog
{
    public interface IWaitDialogCallback
    {
        /// <summary>
        /// Called when cancellation was invoked
        /// </summary>
        void OnCanceled();
    }
}