using System.Collections.Generic;

namespace AnakinRaW.AppUpdaterFramework.Restart;

public interface IDeferredComponentStore
{
    IReadOnlyCollection<object> PendingComponents { get; }
}