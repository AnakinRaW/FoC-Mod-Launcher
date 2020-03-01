using System;

namespace TaskBasedUpdater.TaskRunner
{
    internal class TaskEventArgs : EventArgs
    {
        private bool _cancel;

        public IUpdaterTask Task { get; }

        public bool Cancel
        {
            get => _cancel;
            set => _cancel |= value;
        }

        public TaskEventArgs(IUpdaterTask task)
        {
            Task = task;
        }
    }
}