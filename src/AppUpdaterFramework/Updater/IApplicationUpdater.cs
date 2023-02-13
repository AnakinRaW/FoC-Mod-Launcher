using System;
using System.Threading;
using System.Threading.Tasks;

namespace AnakinRaW.AppUpaterFramework.Updater;

internal interface IApplicationUpdater
{
    event EventHandler<ProgressEventArgs?> Progress;

    Task<object> UpdateAsync(CancellationToken token);
}