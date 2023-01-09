using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;

public static class LibraryInitialization
{
    public static void AddApplicationFramework(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IWindowService>(_ => new WindowService());
        serviceCollection.AddSingleton<IApplicationShutdownService>(sp => new ApplicationShutdownService(sp));

        serviceCollection.AddSingleton<IQueuedDialogService>(sp => new QueuedDialogService(sp));
        serviceCollection.AddSingleton<IModalWindowService>(sp => new ModalWindowService(sp));

        var statusBarService = new StatusBarService();
        serviceCollection.AddSingleton<IStatusBarService>(statusBarService);
        serviceCollection.AddSingleton(statusBarService);
    }
}