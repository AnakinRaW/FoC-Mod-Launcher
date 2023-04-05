using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Updater.Progress;

namespace AnakinRaW.AppUpdaterFramework.Updater;

internal interface IApplicationUpdater
{
    event EventHandler<ComponentProgressEventArgs> Progress;

    Task<UpdateResult> UpdateAsync(CancellationToken token);
}