using System;
using System.Windows;
using System.Windows.Interop;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework;

public class WindowService : IWindowService
{
    private Window _mainWindow;
    private readonly object _syncObject = new();

    public void SetMainWindow(Window window)
    {
        if (_mainWindow is not null)
            throw new InvalidOperationException("Main Window already set.");
        lock (_syncObject)
        {
            if (_mainWindow is not null)
                throw new InvalidOperationException("Main Window already set.");
            _mainWindow = window;
        }
    }

    public void ShowWindow()
    {
        ValidateMainWindow();
        Application.Current.Dispatcher.Invoke(_mainWindow.Show);
    }

    public void SetOwner(Window window)
    {
        ValidateMainWindow();
        if (window is WindowBase windowBase)
        {
            var mwh = new WindowInteropHelper(_mainWindow).Handle;
            windowBase.ChangeOwnerForActivate(mwh);
            windowBase.ChangeOwner(mwh);
        }
        else window.Owner = _mainWindow;
    }

    public void DisableOwner(Window window)
    {
        ValidateMainWindow();
        if (window.Owner == null)
            return;
        window.Owner.IsEnabled = false;
    }

    public void EnableOwner(Window window)
    {
        ValidateMainWindow();
        if (window.Owner == null)
            return;
        window.Owner.IsEnabled = true;
        if (window.Owner.IsActive)
            return;
        window.Owner.Activate();
    }

    private void ValidateMainWindow()
    {
        if (_mainWindow is null)
            throw new InvalidOperationException("No main window set.");
    }
}