using System.Threading;

namespace FocLauncherHost.Updater.Tasks
{
    internal class UpdaterOnCancelTask : UpdaterTask
    {
        public override string ToString()
        {
            return "Updating instance on cancel";
        }

        protected override void ExecuteTask(CancellationToken token)
        {
        }
    }
}