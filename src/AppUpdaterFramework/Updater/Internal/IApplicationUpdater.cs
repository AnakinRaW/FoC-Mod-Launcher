using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Updater.Progress;

namespace AnakinRaW.AppUpaterFramework.Updater;

internal interface IApplicationUpdater
{
    event EventHandler<ProgressEventArgs?> Progress;

    Task<UpdateResult> UpdateAsync(CancellationToken token);
}