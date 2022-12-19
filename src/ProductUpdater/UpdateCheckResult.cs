using Sklavenwalker.ProductUpdater.Catalog;
using Validation;

namespace Sklavenwalker.ProductUpdater;

public record UpdateCheckResult
{
    public static UpdateCheckResult AlreadyInProgress = new()
        { State = UpdateCheckState.AlreadyInProgress, Message = "Checking already running." };
    
    public string? Message { get; init; }

    public IUpdateCatalog? UpdateCatalog { get; }

    public UpdateCheckState State { get; init; }

    private UpdateCheckResult()
    {
    }

    public UpdateCheckResult(IUpdateCatalog catalog)
    {
        Requires.NotNull(catalog, nameof(catalog));
        UpdateCatalog = catalog;
        State = UpdateCheckState.Success;
    }
}