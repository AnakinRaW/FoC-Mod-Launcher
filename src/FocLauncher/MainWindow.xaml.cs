using System;
using System.Windows;
using System.Windows.Controls;
using FocLauncher.Imaging;
using FocLauncher.Themes;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Application;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;
using Sklavenwalker.CommonUtilities.Wpf.Imaging.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Input;
using Sklavenwalker.CommonUtilities.Wpf.Theming;
using Validation;

namespace FocLauncher;

public partial class MainWindow
{
    private readonly IServiceProvider _serviceProvider;

    private ContextMenu _menu;

    public MainWindow(MainWindowViewModel viewModel, IServiceProvider serviceProvider) : base(viewModel, serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        InitializeComponent();

        ImageLibrary.Instance.LoadCatalog(ImageCatalog.Instance);

        BuildContextMenu();
    }

    private void BuildContextMenu()
    {
        var m = new ThemedContextMenu();

        var item = new ThemedMenuItem { Header = "123"};

        var b = ThemedImage.ForMenuItem(Monikers.Undo, item);
        item.Icon = b;
        
        m.Items.Add(item);
        _menu = m;
    }

    protected override void OnContextMenuOpening(ContextMenuEventArgs e)
    {
        e.Handled = ContextMenuPlotter.Instance.ShowContextMenu(_menu, null);
    }

    private void OneChangeTheme(object sender, RoutedEventArgs e)
    {
        var manager = _serviceProvider.GetRequiredService<IThemeManager>();
        if (manager.Theme.Id == "FallbackTheme")
            manager.Theme = new LauncherTheme();
        else
            manager.Theme = new FallbackTheme();
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        var tm = _serviceProvider.GetService<IThemeManager>();
        var c = tm.Theme;
        if (c.Id == "LauncherDefaultTheme")
            tm.Theme = new FallbackTheme();
        else
            tm.Theme = new LauncherTheme();
    }
}