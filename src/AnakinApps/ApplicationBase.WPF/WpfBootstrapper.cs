using AnakinRaW.ApplicationBase.Commands.Handlers;
using AnakinRaW.ApplicationBase.Imaging;
using AnakinRaW.ApplicationBase.Services;
using AnakinRaW.ApplicationBase.Update;
using AnakinRaW.AppUpdaterFramework;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnakinRaW.ApplicationBase;

public abstract class WpfBootstrapper : BootstrapperBase
{
    public virtual ImageKey AppIcon { get; }

    protected override void CreateCoreServicesBeforeEnvironment(IServiceCollection serviceCollection)
    {
        base.CreateCoreServicesBeforeEnvironment(serviceCollection);
        serviceCollection.AddSingleton<IAppResetHandler>(sp => new WpfAppResetHandler(sp));
        serviceCollection.AddTransient<IUnhandledExceptionHandler>(sp => new WpfUnhandledExceptionHandler(sp));
    }

    private protected override void CreateApplicationServices(IServiceCollection serviceCollection)
    {
        base.CreateApplicationServices(serviceCollection);

        serviceCollection.AddApplicationFramework();
        serviceCollection.AddUpdateGui(AppIcon);

        serviceCollection.AddSingleton<IShowUpdateWindowCommandHandler>(sp => new ShowUpdateWindowCommandHandler(sp));

        serviceCollection.AddSingleton(sp => new ApplicationUpdateInteractionFactory(sp));
        serviceCollection.AddSingleton<IUpdateDialogViewModelFactory>(sp => sp.GetRequiredService<ApplicationUpdateInteractionFactory>());

        serviceCollection.TryAddSingleton<IModalWindowFactory>(sp => new ApplicationModalWindowFactory(sp));
        serviceCollection.TryAddSingleton<IDialogFactory>(sp => new ApplicationDialogFactory(sp));

        ImageLibrary.Instance.LoadCatalog(ImageCatalog.Instance);
    }
}