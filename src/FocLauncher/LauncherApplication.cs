using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher.Imaging;
using FocLauncher.Services;
using FocLauncher.Themes;
using FocLauncher.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Controls;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Theming;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;
using Validation;

namespace FocLauncher;

internal class LauncherApplication : ApplicationBase
{
    public LauncherApplication(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        //mainViewModel.InitializeAsync().Wait();
        //LoadStartPageAsync().Forget();
    }

    protected override IApplicationViewModel CreateApplicationViewModel()
    {
        return new ApplicationViewModel(ServiceProvider, new StatusBarViewModel())
        {
            Title = LauncherConstants.ApplicationName,
            IsResizable = false,
            HasMaximizeButton = false,
            HasMinimizeButton = false
        };
    }

    protected override void InitializeResources()
    {
        Resources.MergedDictionaries.Add(LoadResourceValue<ResourceDictionary>("DataTemplates.xaml"));
    }

    protected override void InitializeServices()
    {
        ImageLibrary.Instance.LoadCatalog(ImageCatalog.Instance);
        var themeManager = ServiceProvider.GetRequiredService<IThemeManager>();
        themeManager.Initialize(this, new LauncherTheme());
    }

    protected override ApplicationMainWindow CreateMainWindow(IMainWindowViewModel viewModel)
    {
        return new MainWindow(viewModel, ServiceProvider);
    }

    private async Task LoadStartPageAsync()
    {
        var navigationService = ServiceProvider.GetRequiredService<IViewModelPresenter>();
        navigationService.ShowViewModel(await CreateMainViewModel());
    }

    private Task<IViewModel> CreateMainViewModel()
    {
        return Task.Run(() =>
        {
            return Dispatcher.Invoke<IViewModel>(() => 
                new MainPageViewModel(
                    new GameArgumentsViewModel(ServiceProvider),
                    ServiceProvider));
        });
    }

    internal static T LoadResourceValue<T>(string xamlName)
    {
        return (T)LoadComponent(new Uri(Assembly.GetExecutingAssembly().GetName().Name + ";component/" + xamlName,
            UriKind.Relative));
    }
}