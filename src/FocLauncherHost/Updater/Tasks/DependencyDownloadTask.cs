using System.Threading;
using FocLauncherHost.Updater.MetadataModel;

namespace FocLauncherHost.Updater.Tasks
{
    internal sealed class DependencyDownloadTask : UpdateTask
    {
        public Dependency Dependency { get; }

        public DependencyDownloadTask(Dependency dependency)
        {
            Dependency = dependency;
        }

        protected override void ExecuteTask(CancellationToken token)
        {
        }
    }
}