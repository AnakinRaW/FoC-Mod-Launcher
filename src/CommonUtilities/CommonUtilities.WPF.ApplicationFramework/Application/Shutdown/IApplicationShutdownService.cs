using System;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;

public interface IApplicationShutdownService
{
    IDisposable CreateShutdownPreventionLock(string reasonId);

    void Shutdown(int exitCode);

    void Shutdown(int exitCode, string? message);

    event EventHandler<int>? ShutdownRequested;

    event EventHandler<ShutdownPrevention> ShutdownPrevented;
}