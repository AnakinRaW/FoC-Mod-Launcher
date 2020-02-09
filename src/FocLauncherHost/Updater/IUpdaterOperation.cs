using System.Collections.Generic;
using System.Threading;
using FocLauncherHost.Updater.Tasks;

namespace FocLauncherHost.Updater
{
    internal interface IUpdaterOperation
    {
        IReadOnlyCollection<UpdateTask> DependenciesToDownload { get; }

        IReadOnlyCollection<UpdateTask> DependenciesToInstall { get; }

        IReadOnlyCollection<UpdateTask> DependenciesToRemove { get; }

        long DownloadSize { get; }

        bool Plan();

        void Run(CancellationToken token = default);
    }
}