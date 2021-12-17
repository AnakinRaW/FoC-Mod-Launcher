using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        var b = new ThemedImage
        {
            Moniker = new ImageMoniker { CatalogType = typeof(LauncherImageCatalog), Name = "Test" }
        };

        var item = new MenuItem { Header = "123", Icon = b};
        item.SetResourceReference(StyleProperty, StyleResourceKeys.MenuItemStyleKey);

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

    public static ImageSource ToImageSource(Icon icon)
    {
        ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

        return imageSource;
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

internal class LauncherImageCatalog : ImmutableImageCatalog
{

    private static readonly Lazy<LauncherImageCatalog> LazyConstruction = new(() => new LauncherImageCatalog());

    public static LauncherImageCatalog Instance => LazyConstruction.Value;

    public static IEnumerable<ImageDefinition> Definitions = new List<ImageDefinition>
    {

    };

    private LauncherImageCatalog() : base(Definitions)
    {
    }

}