﻿using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public partial class WindowViewModel : ObservableObject, IWindowViewModel
{
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
}