using System;
using AnakinRaW.ApplicationBase.Utilities;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.ApplicationBase.Services;

internal class UnhandledExceptionHandler : DisposableObject, IUnhandledExceptionHandler
{
    public IServiceProvider Services { get; }
    private readonly ILogger? _logger;

    public UnhandledExceptionHandler(IServiceProvider services)
    {
        Requires.NotNull(services, nameof(services));
        Services = services;
        _logger = services.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            e.ParseUnhandledExceptionObject(out var message);
            var exceptionObject = e.ExceptionObject as Exception;
            if (e.IsTerminating)
                _logger?.LogCritical(exceptionObject, message);
            else
                _logger?.LogError(exceptionObject, message);

            HandleGlobalException(exceptionObject!);
        }
        catch
        {
            // Ignore
        }
    }

    protected virtual void HandleGlobalException(Exception e)
    {
    }

    protected override void DisposeManagedResources()
    {
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
    }
}