using System;
using System.Diagnostics;
using System.Windows;
using AnakinRaW.ApplicationBase.Utilities;
using AnakinRaW.CommonUtilities;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;
using UnhandledExceptionDialogViewModel = AnakinRaW.ApplicationBase.ViewModels.Dialogs.UnhandledExceptionDialogViewModel;

namespace AnakinRaW.ApplicationBase;

public class UnhandledExceptionHandler : DisposableObject
{
    private readonly IServiceProvider _services;
    private readonly ILogger? _logger;

    public UnhandledExceptionHandler(IServiceProvider services)
    {
        Requires.NotNull(services, nameof(services));
        _services = services;
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

            if (Debugger.IsAttached)
                return;

            var owner = Application.Current?.MainWindow;
            var dialog = new UnhandledExceptionDialog(new UnhandledExceptionDialogViewModel(exceptionObject!, _services))
            {
                Owner = owner
            };
            dialog.ShowModal();
        }
        catch
        {
            // Ignore
        }
    }

    protected override void DisposeManagedResources()
    {
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
    }
}