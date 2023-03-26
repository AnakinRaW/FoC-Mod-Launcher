namespace AnakinRaW.ExternalUpdater.CLI;

public enum ExternalUpdaterResult
{
    UpdateFailedNoRestore = -2,
    UpdateFailedWithRestore = -1,
    NoUpdate = 0,
    UpdateSuccess = 1
}