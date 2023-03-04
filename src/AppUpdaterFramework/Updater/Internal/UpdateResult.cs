using System;

namespace AnakinRaW.AppUpdaterFramework.Updater;

public record UpdateResult
{
    public Exception? Exception { get; init; }

    public bool IsCanceled { get; init; }
}