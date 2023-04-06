using System.Windows.Threading;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;

public static class LibraryInitialization
{
    public static void AddApplicationFramework(this IServiceCollection serviceCollection)
    {
        // Must not be lazy!
        serviceCollection.AddSingleton<IAppDispatcher>(new AppDispatcher(Dispatcher.CurrentDispatcher));

        serviceCollection.AddSingleton<IWindowService>(_ => new WindowService());
        serviceCollection.AddSingleton<IApplicationShutdownService>(sp => new ApplicationShutdownService(sp));

        serviceCollection.AddSingleton<IQueuedDialogService>(sp => new QueuedDialogService(sp));
        serviceCollection.AddSingleton<IModalWindowService>(sp => new ModalWindowService(sp));

        serviceCollection.TryAddSingleton<IDialogButtonFactory>(_ => new DialogButtonFactory(true));
        serviceCollection.TryAddSingleton<IThemeManager>(sp => new ThemeManager(sp));
        serviceCollection.TryAddSingleton<IViewModelPresenter>(_ => new ViewModelPresenterService());

        var statusBarService = new StatusBarService();
        serviceCollection.AddSingleton<IStatusBarService>(statusBarService);
        serviceCollection.AddSingleton(statusBarService);
    }
}