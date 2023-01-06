﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace FocLauncher.ViewModels.Designer;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class UpdateWindowViewModel : IUpdateWindowViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }

    public event EventHandler? CloseDialogRequest;
    public WindowState MinMaxState { get; set; }
    public bool RightToLeft { get; set; }
    public string? Title { get; set; }
    public bool IsFullScreen { get; set; }
    public bool IsResizable { get; set; }
    public bool HasMinimizeButton { get; set; }
    public bool HasMaximizeButton { get; set; }
    public bool IsGripVisible { get; set; }
    public bool ShowIcon { get; set; }
    public void CloseDialog()
    {
        throw new NotImplementedException();
    }

    public void OnClosing(CancelEventArgs e)
    {
        throw new NotImplementedException();
    }

    public bool HasDialogFrame { get; set; }
    public bool IsCloseButtonEnabled { get; set; }
    public bool IsLoading { get; }
    public string? LoadingText { get; }
}