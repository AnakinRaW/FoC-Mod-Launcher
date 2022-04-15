namespace Sklavenwalker.ProductUpdater;

public enum UpdateResult
{
    Failed,
    Success,
    SuccessRestartRequired,
    Cancelled,
    NoUpdate
}