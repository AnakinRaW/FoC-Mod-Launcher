using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Controls;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Theming;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Imaging;
using FocLauncher.Themes;
using FocLauncher.Utilities;
using FocLauncher.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher;

internal class LauncherApplication : ApplicationBase
{
    public LauncherApplication(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
    protected override IApplicationViewModel CreateApplicationViewModel()
    {
        return new LauncherViewModel(ServiceProvider, new StatusBarViewModel(ServiceProvider))
        {
            Title = LauncherConstants.ApplicationName,
            IsResizable = true,
            HasMaximizeButton = false,
            HasMinimizeButton = true
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

    protected override void OnApplicationStarted()
    {
        base.OnApplicationStarted();
        LoadMainViewAsync().Forget();
    }

    private async Task LoadMainViewAsync()
    {
        var navigationService = ServiceProvider.GetRequiredService<IViewModelPresenter>();
        navigationService.ShowViewModel(await CreateMainViewModel());
    }

    private Task<IViewModel> CreateMainViewModel()
    {
        return Task.Run(() =>
        {
            return Dispatcher.Invoke<IViewModel>(() => new MainPageViewModel(new GameArgumentsViewModel(ServiceProvider), ServiceProvider));
        });
    }

    internal static T LoadResourceValue<T>(string xamlName)
    {
        return (T)LoadComponent(new Uri(Assembly.GetExecutingAssembly().GetName().Name + ";component/" + xamlName,
            UriKind.Relative));
    }
}