using System;
using System.Threading;
using FocLauncherHost.Updater.TaskRunner;
using FocLauncherHost.Updater.Tasks;

namespace FocLauncherHost.Updater
{
    internal class WaitTask : UpdaterTask
    {
        private readonly AsyncTaskRunner _taskRunner;

        public WaitTask(AsyncTaskRunner taskRunner)
        {
            _taskRunner = taskRunner ?? throw new ArgumentNullException(nameof(taskRunner));
        }

        protected override void ExecuteTask(CancellationToken token)
        {

        }
    }
}