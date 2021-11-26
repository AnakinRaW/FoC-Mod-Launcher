namespace FocLauncher.WaitDialog
{
    /// <summary>
    /// Data Model for an <see cref="IWaitDialog"/>
    /// </summary>
    public class WaitDialogProgressData
    {
        /// <summary>
        /// The current step.
        /// </summary>
        public int CurrentStep { get; }

        /// <summary>
        /// Value indicating whether the dialog is cancelable.
        /// </summary>
        public bool IsCancelable { get; }

        /// <summary>
        /// The progress text.
        /// </summary>
        public string ProgressText { get; }

        /// <summary>
        /// The status bar text.
        /// </summary>
        public string StatusBarText { get; }

        /// <summary>
        /// The number of all steps.
        /// </summary>
        public int TotalSteps { get; }

        /// <summary>
        /// The wait message.
        /// </summary>
        public string WaitMessage { get; }

        public WaitDialogProgressData(string waitMessage, string progressText = null, string statusBarText = null,
            bool isCancelable = false)
            : this(waitMessage, progressText, statusBarText, isCancelable, 0, -1)
        {
        }

        public WaitDialogProgressData(string waitMessage, string progressText, string statusBarText, bool isCancelable,
            int currentStep, int totalSteps)
        {
            WaitMessage = waitMessage;
            ProgressText = progressText;
            StatusBarText = statusBarText;
            IsCancelable = isCancelable;
            CurrentStep = currentStep;
            TotalSteps = totalSteps;
        }

        /// <summary>
        /// Creates a new data model from this instance and increments the current step.
        /// </summary>
        /// <returns>The next data model</returns>
        public WaitDialogProgressData NextStep()
        {
            return new WaitDialogProgressData(WaitMessage, ProgressText, StatusBarText, IsCancelable, CurrentStep + 1,
                TotalSteps);
        }
    }
}