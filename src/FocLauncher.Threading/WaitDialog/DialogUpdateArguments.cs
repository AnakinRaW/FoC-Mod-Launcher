using System;

namespace FocLauncher.WaitDialog
{
    [Serializable]
    public class DialogUpdateArguments
    {
        public string WaitMessage { get; set; }

        public string ProgressMessage { get; set; }

        public bool IsCancellable { get; set; }

        public int CurrentStepCount { get; set; }

        public int TotalStepCount { get; set; }

        public bool ShowMarqueeProgress { get; set; }

        public DialogUpdateArguments()
        {
        }

        public DialogUpdateArguments(string waitMessage, string progressMessage, bool isCancellable)
            : this(waitMessage, progressMessage, isCancellable, 0, 0)
        {
        }

        public DialogUpdateArguments(string waitMessage, string progressMessage, bool isCancellable, int currentStepCount, int totalStepCount)
        {
            WaitMessage = waitMessage;
            ProgressMessage = progressMessage;
            IsCancellable = isCancellable;
            CurrentStepCount = currentStepCount;
            TotalStepCount = totalStepCount;
            ShowMarqueeProgress = totalStepCount <= 0;
        }

        public void Merge(DialogUpdateArguments argsToMerge)
        {
            if (!string.IsNullOrEmpty(argsToMerge.WaitMessage))
                WaitMessage = argsToMerge.WaitMessage;
            ProgressMessage = argsToMerge.ProgressMessage ?? ProgressMessage;
            IsCancellable = argsToMerge.IsCancellable;
            CurrentStepCount = argsToMerge.CurrentStepCount;
            TotalStepCount = argsToMerge.TotalStepCount;
        }
    }
}
