using System.Windows;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class ThemedDialog : DialogWindowBase
{
    private IDialogViewModel? _viewModel;

    public string? ResultButtonId => _viewModel?.ResultButton;

    static ThemedDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedDialog), new FrameworkPropertyMetadata(typeof(ThemedDialog)));
    }

    public void Initialize(IDialogViewModel viewModel)
    {
        Requires.NotNull(viewModel, nameof(viewModel));
        DataContext = viewModel;
        _viewModel = viewModel;
    }
}