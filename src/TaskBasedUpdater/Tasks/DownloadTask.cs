using System;
using System.Threading;

namespace TaskBasedUpdater.Tasks
{
    internal class DownloadTask : SynchronizedUpdaterTask
    {
        internal Uri Uri { get; private set; }

        protected override void SynchronizedInvoke(CancellationToken token)
        {
            
        }
    }
}
