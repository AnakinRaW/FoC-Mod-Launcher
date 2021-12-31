using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Services;
using Validation;

namespace FocLauncher;

internal class LauncherApplication : Application
{
    private readonly IServiceProvider _serviceProvider;

    public LauncherApplication(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var windowViewModel = new MainWindowViewModel(new StatusBarViewModel())
        {
            Title = "Foc Mod Launcher",
            IsResizable = false,
            HasMaximizeButton = false,
            HasMinimizeButton = true
        };
        var window = new MainWindow(windowViewModel, _serviceProvider);
        PreWindowShowInitialize(window);
        MainWindow = window;
        window.Show();
    }

    private void PreWindowShowInitialize(Window window)
    {
        var themeManager = _serviceProvider.GetRequiredService<IThemeManager>();
        themeManager.Initialize(this, window, null);
    }
}