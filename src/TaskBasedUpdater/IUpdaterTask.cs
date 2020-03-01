using System;
using System.Threading;
using TaskBasedUpdater.Component;

namespace TaskBasedUpdater
{
    internal interface IUpdaterTask : IDisposable
    {
        Exception Error { get; }

        IComponent Component { get; }

        void Run(CancellationToken token);
    }
}