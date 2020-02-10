using System.Threading;

namespace FocLauncherHost.Updater.Tasks
{
    internal sealed class DummyTask : UpdaterTask
    {
        protected override void ExecuteTask(CancellationToken token)
        {
        }
    }
}