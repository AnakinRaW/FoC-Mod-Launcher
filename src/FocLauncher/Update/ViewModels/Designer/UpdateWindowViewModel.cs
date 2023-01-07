﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher.Update.ViewModels;
using Sklavenwalker.ProductMetadata;

namespace FocLauncher.Update.ViewModels.Designer;

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
    public IInstalledProductViewModel InstalledProductViewModel { get; set; } = new InstalledProductViewModel();
    public ObservableCollection<ProductBranch> Branches { get; } = new();

    public ProductBranch CurrentBranch { get; set; } =
        new ProductBranch("Test", new Uri("http://example.org", UriKind.Absolute), false);
}