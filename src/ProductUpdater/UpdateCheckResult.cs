﻿using Sklavenwalker.ProductUpdater.Catalog;
using System;

namespace Sklavenwalker.ProductUpdater;

public record UpdateCheckResult
{
    public static UpdateCheckResult AlreadyInProgress = new()
        {State = UpdateCheckState.AlreadyInProgress, Message = "Checking already running."};

    public static UpdateCheckResult Cancelled = new()
        { State = UpdateCheckState.Cancelled, Message = "Update check was cancelled." };

    public Exception? Error { get; init; }

    public string? Message { get; init; }

    public bool IsUpdateAvailable => Error is null && UpdateCatalog is not null && UpdateCatalog.RequiresUpdate();

    public IUpdateCatalog? UpdateCatalog { get; init; }

    public UpdateCheckState State { get; init; }


    public static UpdateCheckResult FromError(Exception e)
    {
        return new UpdateCheckResult { Error = e, Message = e.Message };
    }

    public static UpdateCheckResult Succeeded(IUpdateCatalog catalog)
    {
        return new UpdateCheckResult
        {
            UpdateCatalog = catalog, State = UpdateCheckState.Success,
            Message = "Successfully checked for an update."
        };
    }
}