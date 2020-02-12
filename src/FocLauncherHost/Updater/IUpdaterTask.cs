using System;
using System.Threading;

namespace FocLauncherHost.Updater
{
    internal interface IUpdaterTask : IDisposable
    {
        Exception Error { get; }

        IComponent Component { get; }

        void Run(CancellationToken token);
    }
}