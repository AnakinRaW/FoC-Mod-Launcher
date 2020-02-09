using System;
using System.Threading;

namespace FocLauncherHost.Updater
{
    internal interface IDependencyUpdateTask : IDisposable
    {
        Exception Error { get; }

        void Run(CancellationToken token);
    }
}