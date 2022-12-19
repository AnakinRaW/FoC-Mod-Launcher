using Sklavenwalker.ProductUpdater.Catalog;
using System;
using Validation;

namespace Sklavenwalker.ProductUpdater;

public record UpdateCheckResult
{
    public Exception? Error { get; init; }

    public string? Message { get; init; }

    public bool IsUpdateAvailable => Error is null && UpdateCatalog is not null && UpdateCatalog.RequiresUpdate();

    public IUpdateCatalog? UpdateCatalog { get; init; }

    public UpdateCheckState State { get; init; }

    public UpdateRequest Request { get; }

    public UpdateCheckResult(UpdateRequest request)
    {
        Requires.NotNull(request, nameof(request));
        Request = request;
    }

    internal static UpdateCheckResult Cancelled(UpdateRequest request)
    {
        return new UpdateCheckResult(request) { State = UpdateCheckState.Cancelled, Message = "Update check was cancelled."};
    }

    internal static UpdateCheckResult FromError(UpdateRequest request, Exception e)
    {
        return new UpdateCheckResult(request) { Error = e, Message = e.Message };
    }

    internal static UpdateCheckResult Succeeded(UpdateRequest request, IUpdateCatalog catalog)
    {
        return new UpdateCheckResult(request)
        {
            UpdateCatalog = catalog, State = UpdateCheckState.Success,
            Message = "Successfully checked for an update."
        };
    }
}