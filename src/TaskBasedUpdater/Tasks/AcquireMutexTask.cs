using System.Threading;

namespace TaskBasedUpdater.Tasks
{
    internal class AcquireMutexTask : UpdaterTask
    {
        private Mutex? _mutex;

        internal string MutexName { get; }

        public AcquireMutexTask(string? name = null)
        {
            MutexName = name ?? UpdaterUtilities.UpdaterMutex;
        }

        public override string ToString()
        {
            return $"Acquiring mutex: {MutexName}";
        }

        protected override void ExecuteTask(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            _mutex = UpdaterUtilities.CheckAndSetGlobalMutex(MutexName);
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.Dispose(disposing);
        }
    }
}