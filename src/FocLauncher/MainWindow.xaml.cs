using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using FocLauncher.Themes;
using Microsoft.Extensions.DependencyInjection;
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

    public MainWindow(MainWindowViewModel viewModel, IServiceProvider serviceProvider) : base(viewModel)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        InitializeComponent();

        ImageLibrary.Instance.LoadCatalog(LauncherImageCatalog.Instance);

        BuildContextMenu();
    }

    private void BuildContextMenu()
    {
        var m = new ThemedContextMenu();

        var item = new ThemedMenuItem { Header = "123"};

        var b = ThemedImage.ForMenuItem(Monikers.Undo, item);
        item.Icon = b;

        item.IsEnabled = false;

        m.Items.Add(item);
        _menu = m;
    }

    protected override void OnContextMenuOpening(ContextMenuEventArgs e)
    {
        e.Handled = ContextMenuPlotter.Instance.ShowContextMenu(_menu, null);
    }

    protected override FrameworkElement? CreateStatusBarView()
    {
        var factory = _serviceProvider.GetService<IStatusBarFactory>();
        return factory is null ? null : new WorkerThreadStatusBarContainer(factory);
    }

    private void OneChangeTheme(object sender, RoutedEventArgs e)
    {
        var manager = _serviceProvider.GetRequiredService<IThemeManager>();
        if (manager.Theme.Id == "FallbackTheme")
            manager.Theme = new LauncherTheme();
        else
            manager.Theme = new FallbackTheme();
    }
}

internal static class Monikers
{
    public static ImageMoniker Settings => new() { CatalogType = typeof(LauncherImageCatalog), Name = "Settings" };
    public static ImageMoniker Undo => new() { CatalogType = typeof(LauncherImageCatalog), Name = "Undo" };
}

internal class LauncherImageCatalog : ImmutableImageCatalog
{
    public static ImageDefinition SettingsDefinition => new()
    {
        Kind = ImageFileKind.Xaml,
        Moniker = Monikers.Settings,
        Source = ResourcesUriCreator.Create("Settings_16x", ImageFileKind.Xaml),
        CanTheme = true
    };

    public static ImageDefinition UndoDefinition => new()
    {
        Kind = ImageFileKind.Png,
        Moniker = Monikers.Undo,
        Source = ResourcesUriCreator.Create("Undo_16x", ImageFileKind.Png),
        CanTheme = true
    };


    private static readonly Lazy<LauncherImageCatalog> LazyConstruction = new(() => new LauncherImageCatalog());

    public static LauncherImageCatalog Instance => LazyConstruction.Value;

    public static IEnumerable<ImageDefinition> Definitions = new List<ImageDefinition>
    {
        SettingsDefinition,
        UndoDefinition
    };

    private LauncherImageCatalog() : base(Definitions)
    {
    }

}