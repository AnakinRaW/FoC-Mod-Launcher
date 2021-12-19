using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Sklavenwalker.CommonUtilities.Wpf.Input;
using Sklavenwalker.CommonUtilities.Wpf.NativeMethods;
using Sklavenwalker.CommonUtilities.Wpf.Utils;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class ThemedWindow : ShadowChromeWindow
{
    public WindowViewModel ViewModel { get; protected set; }


    static ThemedWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedWindow), new FrameworkPropertyMetadata(typeof(ThemedWindow)));
        CommandManager.RegisterClassCommandBinding(typeof(UIElement),
            new CommandBinding(ViewCommands.ToggleMaximizeRestoreWindow, OnToggleMaximizeRestoreWindow));
        CommandManager.RegisterClassCommandBinding(typeof(UIElement),
            new CommandBinding(ViewCommands.MinimizeWindow, OnMinimizeWindow));
        CommandManager.RegisterClassCommandBinding(typeof(UIElement),
            new CommandBinding(ViewCommands.CloseWindow, OnCloseWindow));
    }
    
    public ThemedWindow(WindowViewModel dataContext)
    {
        Requires.NotNull(dataContext, nameof(dataContext));
        DataContext = dataContext;
        ViewModel = dataContext;
        DataContextChanged += OnDataContextChanged;
    }

    protected virtual void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not MainWindowViewModel windowViewModel)
            return;
        ViewModel = windowViewModel;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        SetWindowIcon();
    }

    protected T? GetTemplateChild<T>(string name) where T : class
    {
        var templateChild = GetTemplateChild(name) as T;
        try
        {
        }
        catch (InvalidOperationException)
        {
        }
        return templateChild;
    }

    private void SetWindowIcon()
    {
        IconHelper.UseWindowIconAsync(windowIcon => Icon = windowIcon);
    }

    private static void OnCloseWindow(object sender, ExecutedRoutedEventArgs args)
    {
        if (args.Parameter is not Window window)
            return;
        window.Close();
    }

    private static void OnMinimizeWindow(object sender, ExecutedRoutedEventArgs args)
    {
        if (args.Parameter is not Window window)
            return;
        window.WindowState = WindowState.Minimized;
    }

    private static void OnToggleMaximizeRestoreWindow(object sender, ExecutedRoutedEventArgs args)
    {
        if (args.Parameter is not Window window)
            return;
        var handle = new WindowInteropHelper(window).Handle;
        User32.SendMessage(handle, 274, window.WindowState == WindowState.Maximized ? new IntPtr(61728) : new IntPtr(61488), IntPtr.Zero);
    }
}