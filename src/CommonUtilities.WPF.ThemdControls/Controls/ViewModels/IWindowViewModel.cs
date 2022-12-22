﻿using System;
using System.ComponentModel;
using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface IWindowViewModel : INotifyPropertyChanged
{
    event EventHandler CloseDialogRequest;

    WindowState MinMaxState { get; set; }

    bool RightToLeft { get; set; }

    string? Title { get; set; }

    bool IsFullScreen { get; set; }

    bool IsResizable { get; set; }

    bool HasMinimizeButton { get; set; }

    bool HasMaximizeButton { get; set; }

    bool IsGripVisible { get; set; }

    void CloseDialog();

    void OnClosing(CancelEventArgs e);
}