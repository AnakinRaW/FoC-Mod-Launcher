using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.Controls.ViewModels;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public partial class UnhandledExceptionDialog
{
    private readonly IUnhandledExceptionDialogViewModel _viewModel;

    public UnhandledExceptionDialog(IUnhandledExceptionDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
    }

    private void OnCopyStackTrace(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_viewModel.Exception.StackTrace);
    }
}