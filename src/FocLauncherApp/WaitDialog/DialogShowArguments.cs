using System;

namespace FocLauncherApp.WaitDialog
{
    [Serializable]
    internal class DialogShowArguments
    {
        public string Caption { get; set; }

        public bool IsProgressVisible { get; set; }

        public string RootWindowCaption { get; set; }

        public IntPtr ActiveWindowHandle { get; set; }

        public IntPtr AppMainWindowHandle { get; set; }

        public string WaitMessage { get; set; }

        public string ProgressMessage { get; set; }

        public bool IsCancellable { get; set; }

        public int CurrentStepCount { get; set; }

        public int TotalStepCount { get; set; }

        public bool ShowMarqueeProgress { get; set; }

        public DialogShowArguments()
        {
        }

        public DialogShowArguments(string waitMessage, string progressMessage, bool isCancellable, int currentStepCount, int totalStepCount)
        {
            WaitMessage = waitMessage;
            ProgressMessage = progressMessage;
            IsCancellable = isCancellable;
            CurrentStepCount = currentStepCount;
            TotalStepCount = totalStepCount;
            ShowMarqueeProgress = totalStepCount <= 0;
        }

        public DialogShowArguments(string waitMessage, string progressMessage, bool isCancellable)
            : this(waitMessage, progressMessage, isCancellable, 0, 0)
        {
        }

        public DialogShowArguments(string caption, string waitMessage, string progressMessage, bool isCancellable, bool showProgress, int currentStepCount, int totalStepCount)
            : this(waitMessage, progressMessage, isCancellable, currentStepCount, totalStepCount)
        {
            IsProgressVisible = showProgress;
            Caption = caption;
        }

        public void Merge(DialogShowArguments argsToMerge)
        {
            if (!string.IsNullOrEmpty(argsToMerge.WaitMessage))
                WaitMessage = argsToMerge.WaitMessage;
            ProgressMessage = argsToMerge.ProgressMessage ?? ProgressMessage;
            IsCancellable = argsToMerge.IsCancellable;
            CurrentStepCount = argsToMerge.CurrentStepCount;
            TotalStepCount = argsToMerge.TotalStepCount;
        }

        public void SetActiveWindowArgs(string rootWindowCaption, IntPtr activeWindowHandle)
        {
            RootWindowCaption = rootWindowCaption;
            ActiveWindowHandle = activeWindowHandle;
        }
    }
}