namespace AnakinRaW.AppUpdaterFramework.Restart;

internal static class Utilities
{
    public static bool IsRestartRequired(this RestartType restartType)
    {
        return restartType is RestartType.ApplicationRestart or RestartType.ApplicationElevation;
    }
}