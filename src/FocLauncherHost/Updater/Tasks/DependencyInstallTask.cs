using System.Threading;
using FocLauncherHost.Updater.Component;

namespace FocLauncherHost.Updater.Tasks
{
    internal class DependencyInstallTask : SynchronizedUpdaterTask
    {
        internal InstallAction InstallAction { get; private set; }

        internal InstallResult Result { get; set; }

        public DependencyInstallTask(IComponent component)
        {
            Component = component;
        }

        protected override void SynchronizedInvoke(CancellationToken token)
        {
        }
    }
}