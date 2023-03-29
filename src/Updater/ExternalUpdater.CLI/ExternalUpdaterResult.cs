namespace AnakinRaW.ExternalUpdater;

public enum ExternalUpdaterResult
{
    UpdateFailedNoRestore = -2,
    UpdateFailedWithRestore = -1,
    UpdaterNotRun = 0,
    UpdateSuccess = 1,
    Restarted = 2
}