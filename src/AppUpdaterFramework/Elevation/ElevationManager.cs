using System;
using AnakinRaW.AppUpdaterFramework.Updater;

namespace AnakinRaW.AppUpdaterFramework.Elevation;

internal class ElevationManager : IElevationManager
{
    public event EventHandler? ElevationRequested;

    public bool IsElevationRequested { get; }

    public void SetElevationRequest()
    {
        throw new NotImplementedException();
    }
}

internal interface IElevationManager
{
    event EventHandler ElevationRequested;

    bool IsElevationRequested { get; }

    void SetElevationRequest();
}

internal class ElevationRequireException : UpdaterException
{
    public ElevationRequireException()
    {
    }
}