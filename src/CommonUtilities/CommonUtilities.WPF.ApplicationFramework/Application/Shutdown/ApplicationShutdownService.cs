using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework;

internal class ApplicationShutdownService : IApplicationShutdownService
{
    public event EventHandler<int>? ShutdownRequested;

    public event EventHandler<ShutdownPrevention>? ShutdownPrevented;

    private readonly IList<ShutdownPreventionLock> _lockHandles = new List<ShutdownPreventionLock>();

    private readonly ILogger? _logger;

    public ApplicationShutdownService(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public void Shutdown(int exitCode)
    {
        Shutdown(0, null);
    }

    public void Shutdown(int exitCode, string? message)
    {
        ShutdownPreventionLock? shutdownLock;
        lock (_lockHandles) 
            shutdownLock = _lockHandles.FirstOrDefault();
        if (shutdownLock != null)
            ShutdownPrevented?.Invoke(this, new ShutdownPrevention(shutdownLock.ReasonId));
        else
        {
            LogMessage(exitCode, message);
            ShutdownRequested?.Invoke(this, exitCode);
        }
    }

    private void LogMessage(int exitCode, string? message)
    {
        var logText = $"Shutting down the application with exit code {exitCode}. {message ?? "[No message]"}";
        if (exitCode == 0)
            _logger?.LogTrace(logText);
        else
            _logger?.LogWarning(logText);
    }


    public IDisposable CreateShutdownPreventionLock(string? reasonId)
    {
        return new ShutdownPreventionLock(reasonId, this);
    }

    private class ShutdownPreventionLock : IDisposable
    {
        private ApplicationShutdownService _owner;

        public string ReasonId { get; }

        public ShutdownPreventionLock(string reasonId, ApplicationShutdownService owner)
        {
            ReasonId = reasonId;
            lock (owner._lockHandles) 
                owner._lockHandles.Add(this);
            _owner = owner;
        }

        public void Dispose()
        {
            lock (_owner._lockHandles)
                _owner._lockHandles.Remove(this);
            _owner = null!;
        }
    }
}