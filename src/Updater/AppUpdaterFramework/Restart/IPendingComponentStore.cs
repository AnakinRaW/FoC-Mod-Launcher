using System.Collections.Generic;

namespace AnakinRaW.AppUpdaterFramework.Restart;

internal interface IPendingComponentStore
{
    IReadOnlyCollection<object> PendingComponents { get; }
}