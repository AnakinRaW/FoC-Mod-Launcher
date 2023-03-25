using AnakinRaW.AppUpdaterFramework.Commands.Handlers;
using AnakinRaW.AppUpdaterFramework.Imaging;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.AppUpdaterFramework.ViewModels.Factories;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnakinRaW.AppUpdaterFramework;

public static class LibraryInitialization
{
    public static void AddUpdateGui(this IServiceCollection serviceCollection, ImageKey applicationIcon = default)
    {
        serviceCollection.AddUpdateFramework();

        serviceCollection.AddSingleton<IProductViewModelFactory>(sp => new ProductViewModelFactory(sp));
        serviceCollection.AddSingleton<IUpdateCommandHandler>(sp => new UpdateCommandHandler(sp));

        serviceCollection.Replace(ServiceDescriptor.Scoped<IInteractionHandler>(sp => new DialogInteractionHandler(sp)));



        ImageLibrary.Instance.LoadCatalog(ImageCatalog.Instance);
        AppIconHolder.ApplicationIcon = applicationIcon;
    }
}