using System;
using System.Windows;
using System.Windows.Threading;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class ThemedDialog : ModalWindow
{
    private IDialogViewModel? _viewModel;
    
    static ThemedDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedDialog), new FrameworkPropertyMetadata(typeof(ThemedDialog)));
    }

    public ThemedDialog()
    {
        DataContextChanged += OnDataContextChanged;
    }
    
    public void Initialize(IDialogViewModel viewModel)
    {
        Requires.NotNull(viewModel, nameof(viewModel));
        DataContext = viewModel;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.CloseDialogRequest -= OnCloseRequested;
        if (e.NewValue is IDialogViewModel dialogViewModel)
        {
            _viewModel = dialogViewModel;
            _viewModel.CloseDialogRequest += OnCloseRequested;
        }
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(DispatcherPriority.Input, Close);
    }
}