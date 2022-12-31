using System;
using System.Windows;
using System.Windows.Controls;
using FocLauncher.Commands;
using FocLauncher.Themes;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Theming;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Sklavenwalker.CommonUtilities.Wpf.Input;
using Validation;

namespace FocLauncher;

public partial class MainWindow
{
    private readonly IServiceProvider _serviceProvider;

    private ContextMenu? _menu;

    public MainWindow(IMainWindowViewModel viewModel, IServiceProvider serviceProvider) : base(viewModel, serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        InitializeComponent();
        
        BuildContextMenu();
    }

    private void BuildContextMenu()
    {
        var c = new TestCommand();

        var subMenu = MenuDefinition.Builder("Menu", enable: true, tooltip: "Test")
            .AddItem(ButtonDefinition.FromCommandDefinition(c))
            .Build();

        var cModel = ContextMenuDefinition.Builder()
            .AddItem(ButtonDefinition.FromCommandDefinition(c))
            .AddSeparator()
            .AddItem(subMenu)
            .Build();

        var p = new ModelBasedContextMenuProvider();
        _menu = p.Provide(cModel);
    }

    protected override void OnContextMenuOpening(ContextMenuEventArgs e)
    {
        if (_menu is null)
            return;
        e.Handled = ContextMenuPlotter.Instance.ShowContextMenu(_menu, null);
    }

    private void OneChangeTheme(object sender, RoutedEventArgs e)
    {
        var manager = _serviceProvider.GetRequiredService<IThemeManager>();
        if (manager.Theme.Id == "DefaultTheme")
            manager.Theme = new LauncherTheme();
        else
            manager.Theme = new DefaultTheme();
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        var tm = _serviceProvider.GetService<IThemeManager>();
        var c = tm.Theme;
        if (c.Id == "LauncherDefaultTheme")
            tm.Theme = new DefaultTheme();
        else
            tm.Theme = new LauncherTheme();
    }
}