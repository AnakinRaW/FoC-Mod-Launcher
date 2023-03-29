using System.Collections.Generic;

namespace AnakinRaW.AppUpdaterFramework.Restart;

internal interface IPendingComponentStore
{
    IReadOnlyCollection<PendingComponent> PendingComponents { get; }
}