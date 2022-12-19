using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher.Services;
using FocLauncher.Themes;
using FocLauncher.Threading;
using FocLauncher.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Services;
using Sklavenwalker.CommonUtilities.Wpf.Theming;
using Validation;

namespace FocLauncher;

internal class LauncherApplication : Application
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;
    private bool _windowShown;
    private readonly object _syncObject = new();

    public LauncherApplication(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var mainViewModel = new ApplicationViewModel(_serviceProvider, new StatusBarViewModel())
        {
            Title = LauncherConstants.ApplicationName,
            IsResizable = false,
            HasMaximizeButton = false,
            HasMinimizeButton = true
        };
        PreWindowShowInitialize();
        var window = InitializeWindow(mainViewModel);
        ShowWindow(window);
        mainViewModel.InitializeAsync().Wait();
        LoadStartPageAsync().Forget();
    }

    private Window InitializeWindow(MainWindowViewModel viewModel)
    {
        var window = new MainWindow(viewModel, _serviceProvider);
        MainWindow = window;
        return window;
    }

    private void PreWindowShowInitialize()
    {
        Resources.MergedDictionaries.Add(LoadResourceValue<ResourceDictionary>("DataTemplates.xaml"));
        var themeManager = _serviceProvider.GetRequiredService<IThemeManager>();
        themeManager.Initialize(this, new LauncherTheme());
        ScrollBarThemingUtilities.Initialize(themeManager);
    }
    
    private void ShowWindow(Window window)
    {
        if (_windowShown)
            return;
        lock (_syncObject)
        {
            if (_windowShown)
                return;
            _windowShown = true;
        }
        _logger?.LogTrace("Showing the window.");
        Dispatcher.Invoke(window.Show);
    }

    private async Task LoadStartPageAsync()
    {
        var navigationService = _serviceProvider.GetRequiredService<IViewModelPresenter>();
        navigationService.ShowViewModel(await CreateMainViewModel());
    }

    private Task<ILauncherViewModel> CreateMainViewModel()
    {
        return Task.Run(() =>
        {
            return Dispatcher.Invoke<ILauncherViewModel>(() => 
                new MainPageViewModel(
                    new GameArgumentsViewModel(_serviceProvider),
                    _serviceProvider));
        });
    }

    internal static T LoadResourceValue<T>(string xamlName)
    {
        return (T)LoadComponent(new Uri(Assembly.GetExecutingAssembly().GetName().Name + ";component/" + xamlName,
            UriKind.Relative));
    }
}