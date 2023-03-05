using System;

namespace AnakinRaW.AppUpdaterFramework.Restart;

internal sealed class RestartManager : IRestartManager
{
    public event EventHandler<EventArgs>? RebootRequired;

    public RestartType RequiredRestartType { get; private set; }

    public void SetRestart(RestartType restartType)
    {
        if (restartType.IsRestartRequired())
            OnRebootRequired();
        if (ShouldOverride()) 
            RequiredRestartType = restartType;
    }

    private bool ShouldOverride()
    {
        return RequiredRestartType switch
        {
            RestartType.None => true,
            RestartType.ApplicationRestart => false,
            _ => false
        };
    }

    private void OnRebootRequired()
    {
        RebootRequired?.Invoke(this, EventArgs.Empty);
    }
}