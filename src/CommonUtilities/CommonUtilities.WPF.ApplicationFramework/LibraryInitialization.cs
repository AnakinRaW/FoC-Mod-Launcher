using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework;

public static class LibraryInitialization
{
    public static void AddApplicationFramework(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IWindowService>(_ => new WindowService());
        serviceCollection.AddSingleton<IApplicationShutdownService>(sp => new ApplicationShutdownService(sp));

        serviceCollection.AddSingleton<IQueuedDialogService>(sp => new QueuedDialogService(sp));
    }
}