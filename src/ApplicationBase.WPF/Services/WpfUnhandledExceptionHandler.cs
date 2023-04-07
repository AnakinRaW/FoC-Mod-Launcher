using System;
using System.Diagnostics;
using System.Windows;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using UnhandledExceptionDialogViewModel = AnakinRaW.ApplicationBase.ViewModels.Dialogs.UnhandledExceptionDialogViewModel;

namespace AnakinRaW.ApplicationBase.Services;

internal class WpfUnhandledExceptionHandler : UnhandledExceptionHandler
{
    public WpfUnhandledExceptionHandler(IServiceProvider services) : base(services)
    {
    }

    protected override void HandleGlobalException(Exception e)
    {
        if (Debugger.IsAttached)
            return;

        var owner = Application.Current?.MainWindow;
        var dialog = new UnhandledExceptionDialog(new UnhandledExceptionDialogViewModel(e, Services))
        {
            Owner = owner
        };
        dialog.ShowModal();
    }
}