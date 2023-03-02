using System;

namespace AnakinRaW.AppUpaterFramework.Updater;

public record UpdateResult
{
    public Exception? Exception { get; init; }

    public bool IsCanceled { get; init; }
}