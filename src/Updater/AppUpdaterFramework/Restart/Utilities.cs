namespace AnakinRaW.AppUpdaterFramework.Restart;

internal static class Utilities
{
    public static bool IsRestartRequired(this RestartType restartType)
    {
        return restartType == RestartType.ApplicationRestart;
    }
}