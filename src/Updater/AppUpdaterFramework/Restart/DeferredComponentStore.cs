using System;
using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Restart;

internal class DeferredComponentStore : IWritableDeferredComponentStore
{
    public IReadOnlyCollection<object> PendingComponents { get; }

    public DeferredComponentStore(IServiceProvider serviceProvider)
    {
        
    }

    public void AddComponent(IInstallableComponent component)
    {
    }

    public void Clear()
    {
    }
}