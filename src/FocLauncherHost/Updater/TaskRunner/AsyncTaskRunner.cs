using System;
using System.Collections.Concurrent;
using System.Threading;

namespace FocLauncherHost.Updater.TaskRunner
{
    internal class AsyncTaskRunner : TaskRunner
    {
        private readonly ConcurrentBag<Exception> _exceptions;
        private readonly ManualResetEvent[] _handles;
        private CancellationToken _cancel;

        internal int WorkerCount { get; }

        internal AggregateException Exception => _exceptions.Count > 0 ? new AggregateException(_exceptions) : null;

        public AsyncTaskRunner(int workerCount)
        {
            if (workerCount < 1)
                throw new ArgumentOutOfRangeException(nameof(workerCount));
            WorkerCount = workerCount;
            _exceptions = new ConcurrentBag<Exception>();
            _handles = new ManualResetEvent[workerCount];
            for (var index = 0; index < _handles.Length; ++index)
                _handles[index] = new ManualResetEvent(false);
        }

        public void Wait()
        {
            Wait(Timeout.InfiniteTimeSpan);
            var exception = Exception;
            if (exception != null)
                throw exception;
        }

        internal void Wait(TimeSpan timeout)
        {
            if (!WaitHandle.WaitAll(_handles, timeout))
                throw new TimeoutException();
        }

        protected override void Invoke(CancellationToken token)
        {
            ThrowIfCancelled(token);
            Tasks.AddRange(TaskQueue);
            _cancel = token;
            var threads = new Thread[WorkerCount];
            for (var index = 0; index < WorkerCount; ++index)
            {
                threads[index] = new Thread(InvokeThreaded)
                {
                    IsBackground = true,
                    Name = $"AsyncTaskRunnerThread{index}"
                };
                threads[index].Start(_handles[index]);
            }
        }

        private void InvokeThreaded(object obj)
        {
            var manualResetEvent = (ManualResetEvent)obj;
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancel);
            try
            {
                var canceled = false;
                while (TaskQueue.TryDequeue(out var task))
                {
                    try
                    {
                        ThrowIfCancelled(_cancel);
                        task.Run(_cancel);
                    }
                    catch (Exception ex)
                    {
                        _exceptions.Add(ex);
                        if (!canceled)
                        {
                            if (ex.IsExceptionType<OperationCanceledException>())
                                Logger.Trace($"Activity threw exception {ex.GetType()}: {ex.Message}" + Environment.NewLine + $"{ex.StackTrace}");
                            else
                                Logger.Error(ex, $"Activity threw exception {ex.GetType()}: {ex.Message}");
                        }
                        var e = new TaskEventArgs(task)
                        {
                            Cancel = _cancel.IsCancellationRequested || IsCancelled || ex.IsExceptionType<OperationCanceledException>()
                        };
                        OnError(e);
                        if (e.Cancel)
                        {
                            canceled = true;
                            linkedTokenSource.Cancel();
                        }
                    }
                }
            }
            finally
            {
                manualResetEvent.Set();
            }
        }
    }
}