using System;
using System.Threading;
using FocLauncherHost.Updater.TaskRunner;

namespace FocLauncherHost.Updater.Tasks
{
    internal class WaitTask : UpdaterTask
    {
        private readonly AsyncTaskRunner _taskRunner;

        public WaitTask(AsyncTaskRunner taskRunner)
        {
            _taskRunner = taskRunner ?? throw new ArgumentNullException(nameof(taskRunner));
        }

        public override string ToString()
        {
            return "Waiting for other activities";
        }

        protected override void ExecuteTask(CancellationToken token)
        {
            try
            {
                _taskRunner.Wait();
            }
            catch
            {
                Logger.Trace("Wait activity is stopping all subsequent activities...");
                throw new StopTaskRunnerException();
            }
        }
    }
}