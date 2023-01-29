using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using AnakinRaW.CommonUtilities.Wpf.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public partial class WindowViewModel : ObservableObject, IWindowViewModel
{
    public event EventHandler? CloseDialogRequest;

    [ObservableProperty]
    private WindowState _minMaxState;

    [ObservableProperty]
    private bool _rightToLeft;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private bool _isFullScreen;

    [ObservableProperty]
    private bool _isResizable = true;

    [ObservableProperty]
    private bool _hasMaximizeButton = true;

    [ObservableProperty]
    private bool _hasMinimizeButton = true;

    [ObservableProperty]
    private bool _isGripVisible = true;

    [ObservableProperty]
    private bool _showIcon = true;
    public void CloseDialog()
    {
        OnCloseRequested();
    }

    public virtual void OnClosing(CancelEventArgs e)
    {
    }

    protected virtual void OnCloseRequested()
    {
        CloseDialogRequest?.Invoke(this, EventArgs.Empty);
    }
}