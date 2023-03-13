using System;
using AnakinRaW.AppUpdaterFramework.Restart;

namespace AnakinRaW.AppUpdaterFramework.Updater;

public record UpdateResult
{
    public Exception? Exception { get; init; }

    public bool IsCanceled { get; init; }

    public bool FailedRestore { get; init; }

    public bool RequiresElevation { get; init; }

    public RestartType RestartType { get; init; }



}