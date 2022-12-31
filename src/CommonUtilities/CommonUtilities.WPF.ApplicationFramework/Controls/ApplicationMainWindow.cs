using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Converters;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Controls;

public class ApplicationMainWindow : ThemedWindow
{ 
    public IServiceProvider ServiceProvider { get; }

    public new IMainWindowViewModel ViewModel { get; }

    private ContentControl? _statusBarHost;

    protected readonly ILogger? Logger;

    static ApplicationMainWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ApplicationMainWindow), new FrameworkPropertyMetadata(typeof(ApplicationMainWindow)));
        Keyboard.DefaultRestoreFocusMode = RestoreFocusMode.None;
    }

    public ApplicationMainWindow(IMainWindowViewModel viewModel, IServiceProvider serviceProvider) : base(viewModel)
    {
        ViewModel = viewModel;
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        serviceProvider.GetRequiredService<StatusBarService>().StatusBarModel = viewModel.StatusBar;
        SetBindings();
        DataContextChanged += OnDataContextChanged;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _statusBarHost = GetTemplateChild<ContentControl>("PART_StatusBarHost")!;
        if (ViewModel.StatusBar.IsVisible)
        {
            _statusBarHost.Content = CreateStatusBarView();
        }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not IMainWindowViewModel mainWindowViewModel)
            return;
        _statusBarHost!.Content = !mainWindowViewModel.StatusBar.IsVisible ? null : CreateStatusBarView();
    }

    private FrameworkElement? CreateStatusBarView()
    {
        var factory = ServiceProvider.GetService<IStatusBarFactory>();
        if (factory == null)
        {
            Logger?.LogTrace("No IStatusBarFactory registered.");
            return null;
        }
        return factory.CreateStatusBar(ViewModel.StatusBar);
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