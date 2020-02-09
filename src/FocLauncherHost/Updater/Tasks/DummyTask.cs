using System.Threading;

namespace FocLauncherHost.Updater.Tasks
{
    internal sealed class DummyTask : UpdateTask
    {
        protected override void ExecuteTask(CancellationToken token)
        {
        }
    }
}