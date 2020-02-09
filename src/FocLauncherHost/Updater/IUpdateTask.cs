using System;
using System.Threading;
using FocLauncherHost.Updater.MetadataModel;

namespace FocLauncherHost.Updater
{
    internal interface IUpdateTask : IDisposable
    {
        Exception Error { get; }

        Dependency Dependency { get; }

        void Run(CancellationToken token);
    }
}