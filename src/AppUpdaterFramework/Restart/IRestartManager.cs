using System;

namespace AnakinRaW.AppUpdaterFramework.Restart;

internal interface IRestartManager
{
    event EventHandler<EventArgs> RebootRequired;

    RestartType RequiredRestartType { get; }

    void SetRestart(RestartType restartType);
}