using System;
using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Restart;

internal class PendingComponentStore : IWritablePendingComponentStore
{
    public IReadOnlyCollection<object> PendingComponents { get; }

    public PendingComponentStore(IServiceProvider serviceProvider)
    {
        
    }

    public void AddComponent(IInstallableComponent component)
    {
    }

    public void Clear()
    {
    }
}