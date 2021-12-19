using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class WindowViewModel : ObservableObject
{
    private WindowState _minMaxState;
    private bool _leftToRight;
    private string? _title;
    private bool _isFullScreen;
    private bool _isResizable = true;
    private bool _hasMaximizeButton = true;
    private bool _hasMinimizeButton = true;
    private bool _isGripVisible = true;

    public bool RightToLeft
    {
        get => _leftToRight;
        set => SetProperty(ref _leftToRight, value);
    }

    public WindowState MinMaxState
    {
        get => _minMaxState;
        set => SetProperty(ref _minMaxState, value);
    }

    public string? Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public bool IsFullScreen
    {
        get => _isFullScreen;
        set => SetProperty(ref _isFullScreen, value);
    }

    public bool IsResizable
    {
        get => _isResizable;
        set => SetProperty(ref _isResizable, value);
    }

    public bool HasMaximizeButton
    {
        get => _hasMaximizeButton;
        set => SetProperty(ref _hasMaximizeButton, value);
    }

    public bool HasMinimizeButton
    {
        get => _hasMinimizeButton;
        set => SetProperty(ref _hasMinimizeButton, value);
    }

    public bool IsGripVisible
    {
        get => _isGripVisible;
        set => SetProperty(ref _isGripVisible, value);
    }
}