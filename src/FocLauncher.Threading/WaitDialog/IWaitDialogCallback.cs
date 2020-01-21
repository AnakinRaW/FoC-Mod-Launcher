namespace FocLauncher.WaitDialog
{
    public interface IWaitDialogCallback
    {
        /// <summary>
        /// Called when cancellation was invoked
        /// </summary>
        void OnCanceled();
    }
}