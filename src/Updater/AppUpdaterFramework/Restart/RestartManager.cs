using System;

namespace AnakinRaW.AppUpdaterFramework.Restart;

internal sealed class RestartManager : IRestartManager
{
    public event EventHandler<EventArgs>? RestartRequired;

    public RestartType RequiredRestartType { get; private set; }

    public void SetRestart(RestartType restartType)
    {
        if (RequiredRestartType == restartType)
            return;

        if (ShouldOverride(restartType))
        {
            RequiredRestartType = restartType;
            if (restartType.IsRestartRequired())
                OnRebootRequired();
        }
    }

    private bool ShouldOverride(RestartType newRestartType)
    {
        return RequiredRestartType switch
        {
            RestartType.None => true,
            RestartType.ApplicationRestart => newRestartType > RestartType.ApplicationRestart,
            RestartType.ApplicationElevation => false,
            _ => false
        };
    }

    private void OnRebootRequired()
    {
        RestartRequired?.Invoke(this, EventArgs.Empty);
    }
}