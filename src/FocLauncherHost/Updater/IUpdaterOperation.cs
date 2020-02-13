using System.Collections.Generic;
using System.Threading;
using FocLauncherHost.Updater.Tasks;

namespace FocLauncherHost.Updater
{
    internal interface IUpdaterOperation
    {
        IReadOnlyCollection<ComponentDownloadTask> DependenciesToDownload { get; }

        IReadOnlyCollection<UpdaterTask> DependenciesToInstall { get; }

        IReadOnlyCollection<UpdaterTask> DependenciesToRemove { get; }

        long DownloadSize { get; }

        bool Plan();

        void Run(CancellationToken token = default);
    }
}