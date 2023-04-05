using AnakinRaW.ApplicationBase.Commands.Handlers;
using AnakinRaW.ApplicationBase.Imaging;
using AnakinRaW.AppUpdaterFramework;

using AnakinRaW.CommonUtilities.Wpf.Imaging;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.ApplicationBase;

public static class LibraryInitialization
{
    public static void AddApplicationBase(this IServiceCollection serviceCollection, ImageKey applicationIcon = default)
    {
        serviceCollection.AddSingleton<IShowUpdateWindowCommandHandler>(sp => new ShowUpdateWindowCommandHandler(sp));

        serviceCollection.AddUpdateGui(applicationIcon);
        ImageLibrary.Instance.LoadCatalog(ImageCatalog.Instance);
    }
}