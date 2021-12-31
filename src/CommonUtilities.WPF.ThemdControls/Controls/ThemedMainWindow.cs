using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shell;
using Sklavenwalker.CommonUtilities.Wpf.Converters;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class ThemedMainWindow : ThemedWindow
{
    private ContentControl _statusBarHost;

    static ThemedMainWindow()
    {
        Keyboard.DefaultRestoreFocusMode = RestoreFocusMode.None;
    }

    public ThemedMainWindow(IMainWindowViewModel viewModel) : base(viewModel)
    {
        Requires.NotNull(viewModel, nameof(viewModel));
        SetBindings();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _statusBarHost = GetTemplateChild<ContentControl>("PART_StatusBarHost")!;
        if (ViewModel is IMainWindowViewModel mwv && mwv.StatusBar.IsVisible)
            _statusBarHost.Content = CreateStatusBarView();
    }

    protected override void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        base.OnDataContextChanged(sender, e);
        if (e.NewValue is not IMainWindowViewModel mainWindowViewModel)
            return;
        _statusBarHost.Content = !mainWindowViewModel.StatusBar.IsVisible ? null : CreateStatusBarView();
    }

    protected virtual FrameworkElement? CreateStatusBarView()
    {
        return null;
    }

    private void SetBindings()
    {
        TaskbarItemInfo = new TaskbarItemInfo();
        BindingOperations.SetBinding(TaskbarItemInfo, TaskbarItemInfo.ProgressStateProperty, new Binding
        {
            Path = new PropertyPath("ProgressState"),
            Converter = new BiDirectionalEnumConverter()
        });
    }
}