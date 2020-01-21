namespace FocLauncher.WaitDialog
{
    public interface IWaitDialog
    {
        /// <summary>
        /// Ends the wait dialog.
        /// </summary>
        /// <param name="canceled">if set to <see langword="true"/> the task was canceled by the user.</param>
        void EndWaitDialog(out bool canceled);

        /// <summary>
        /// Determines whether the task was canceled by the user.
        /// </summary>
        /// <param name="canceled">if set to <see langword="true"/> the task was canceled by the user.</param>
        void HasCanceled(out bool canceled);

        /// <summary>
        /// Starts the wait dialog and executes the Action/Function.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="waitMessage">The wait message.</param>
        /// <param name="progressText">The progress text.</param>
        /// <param name="delayToShowDialog">The delay to show dialog.</param>
        /// <param name="isCancelable">if set to <see langword="true"/> the task can be canceled by the user.</param>
        /// <param name="showMarqueeProgress">if set to <see langword="true"/> the progress bar shows indeterminate.</param>
        bool StartWaitDialog(string caption, string waitMessage, string progressText, int delayToShowDialog,
            bool isCancelable, bool showMarqueeProgress);

        /// <summary>
        /// Starts the wait dialog with callback and executes the Action/Function.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="waitMessage">The wait message.</param>
        /// <param name="progressText">The progress text.</param>
        /// <param name="isCancelable">if set to <see langword="true"/> the task can be canceled by the user.</param>
        /// <param name="delayToShowDialog">The delay to show dialog.</param>
        /// <param name="showProgress">if set to <see langword="true"/> the progress bar will be shown</param>
        /// <param name="totalSteps">The total steps.</param>
        /// <param name="currentStep">The current step.</param>
        /// <param name="callback">The callback.</param>
        void StartWaitDialogWithCallback(string caption, string waitMessage, string progressText, bool isCancelable,
            int delayToShowDialog, bool showProgress, int totalSteps, int currentStep, IWaitDialogCallback callback);

        /// <summary>
        /// Starts the wait dialog with percentage progress and executes the Action/Function.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="waitMessage">The wait message.</param>
        /// <param name="statusBarText">The status bar text.</param>
        /// <param name="delayToShowDialog">The delay to show dialog.</param>
        /// <param name="isCancelable">if set to <see langword="true"/> the task can be canceled by the user.</param>
        /// <param name="totalSteps">The total steps.</param>
        /// <param name="currentStep">The current step.</param>
        void StartWaitDialogWithPercentageProgress(string caption, string waitMessage, string statusBarText,
            int delayToShowDialog, bool isCancelable, int totalSteps, int currentStep);

        /// <summary>
        /// Updates the progress.
        /// </summary>
        /// <param name="waitMessage">The wait message.</param>
        /// <param name="progressText">The progress text.</param>
        /// <param name="currentStep">The current step.</param>
        /// <param name="totalSteps">The total steps.</param>
        /// <param name="disableCancel">if set to <see langword="true"/> the dialog hides the cancellation button</param>
        /// <param name="canceled">if set to <see langword="true"/> the task was canceled</param>
        void UpdateProgress(string waitMessage, string progressText, int currentStep, int totalSteps,
            bool disableCancel, out bool canceled);
    }
}