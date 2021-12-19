using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class ThemedMainWindow : ThemedWindow
{
    private ContentControl _statusBarHost;

    static ThemedMainWindow()
    {
        Keyboard.DefaultRestoreFocusMode = RestoreFocusMode.None;
    }

    public ThemedMainWindow(MainWindowViewModel viewModel) : base(viewModel)
    {
        Requires.NotNull(viewModel, nameof(viewModel));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _statusBarHost = GetTemplateChild<ContentControl>("PART_StatusBarHost")!;
        if (ViewModel is MainWindowViewModel mwv && mwv.StatusBar.IsVisible)
            _statusBarHost.Content = CreateStatusBarView();
    }

    protected override void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        base.OnDataContextChanged(sender, e);
        if (e.NewValue is not MainWindowViewModel mainWindowViewModel)
            return;
        _statusBarHost.Content = !mainWindowViewModel.StatusBar.IsVisible ? null : CreateStatusBarView();
    }

    protected virtual FrameworkElement? CreateStatusBarView()
    {
        return null;
    }
}