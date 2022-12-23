﻿using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public partial class UnhandledExceptionDialog
{
    private readonly IUnhandledExceptionDialogViewModel _viewModel;

    public UnhandledExceptionDialog(IUnhandledExceptionDialogViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
    }

    private void OnCopyStackTrace(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_viewModel.Exception.StackTrace);
    }
}