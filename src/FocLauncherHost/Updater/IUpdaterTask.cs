using System;
using System.Threading;

namespace FocLauncherHost.Updater
{
    internal interface IUpdaterTask : IDisposable
    {
        Exception Error { get; }

        IDependency Dependency { get; }

        void Run(CancellationToken token);
    }
}