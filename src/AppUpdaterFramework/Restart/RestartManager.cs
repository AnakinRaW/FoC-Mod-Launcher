using System;

namespace AnakinRaW.AppUpaterFramework.Restart;

internal interface IRestartManager
{
    event EventHandler<EventArgs> RebootRequired;

    RestartType RequiredRestartType { get; }

    void SetRestart(RestartType restartType);
}

internal class RestartManager : IRestartManager
{
    public event EventHandler<EventArgs>? RebootRequired;
    public RestartType RequiredRestartType { get; }
    public void SetRestart(RestartType restartType)
    {
    }
}

internal enum RestartType
{
    None,
    ApplicationRestart
}