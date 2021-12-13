using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FocLauncher.Themes;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
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

        BuildContextMenu();
    }

    private void BuildContextMenu()
    {
        var m = new ThemedContextMenu();

        using var iconStream = Application.GetResourceStream(
            new Uri("pack://application:,,,/FocLauncher;component/Resources/icon.ico", UriKind.Absolute))?.Stream;
        var i = new Icon(iconStream);
        var b = new System.Windows.Controls.Image
        {
            Source = ToImageSource(i)
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