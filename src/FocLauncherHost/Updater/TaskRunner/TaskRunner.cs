using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using NLog;

namespace FocLauncherHost.Updater.TaskRunner
{
    internal class TaskRunner : IEnumerable<IUpdaterTask>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly List<IUpdaterTask> _tasks;

        public event EventHandler<TaskEventArgs> Error;

        protected ConcurrentQueue<IUpdaterTask> TaskQueue { get; }

        internal IList<IUpdaterTask> Tasks => _tasks;

        internal bool IsCancelled { get; private set; }

        public TaskRunner()
        {
            TaskQueue = new ConcurrentQueue<IUpdaterTask>();
            _tasks = new List<IUpdaterTask>();
        }

        public void Run(CancellationToken token)
        {
            Invoke(token);
        }

        public void Queue(IUpdaterTask activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));
            TaskQueue.Enqueue(activity);
        }

        public IEnumerator<IUpdaterTask> GetEnumerator()
        {
            return TaskQueue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return TaskQueue.GetEnumerator();
        }

        protected virtual void Invoke(CancellationToken token)
        {
            var alreadyCancelled = false;
            _tasks.AddRange(TaskQueue);
            while (TaskQueue.TryDequeue(out var task))
            {
                try
                {
                    ThrowIfCancelled(token);
                    task.Run(token);
                }
                catch (StopTaskRunnerException)
                {
                    Logger.Trace("Stop subsequent tasks");
                    break;
                }
                catch (Exception e)
                {
                    if (!alreadyCancelled)
                    {
                        if (e.IsExceptionType<OperationCanceledException>())
                            Logger.Trace($"Task {task} cancelled");
                        else
                            Logger.Error(e, $"Task {task} threw an exception: {e.GetType()}: {e.Message}");
                    }

                    var error = new TaskEventArgs(task)
                    {
                        Cancel = token.IsCancellationRequested || IsCancelled ||
                                 e.IsExceptionType<OperationCanceledException>()
                    };
                    if (error.Cancel)
                        alreadyCancelled = true;
                    OnError(error);
                }
            }
        }

        protected virtual void OnError(TaskEventArgs e)
        { 
            Error?.Invoke(this, e);
            if (!e.Cancel)
                return;
            IsCancelled |= e.Cancel;
        }

        protected void ThrowIfCancelled(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (IsCancelled)
                throw new OperationCanceledException(token);
        }
    }
}