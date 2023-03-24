using System;

namespace AnakinRaW.AppUpdaterFramework.Restart;

internal interface IRestartManager
{
    event EventHandler<EventArgs> RestartRequired;

    RestartType RequiredRestartType { get; }

    void SetRestart(RestartType restartType);
}