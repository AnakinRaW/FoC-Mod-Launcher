using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.AppUpaterFramework.Updater;

namespace FocLauncher.Update.ViewModels;

internal interface IProductViewModelFactory
{
    IProductViewModel Create(IInstalledProduct product, IUpdateCatalog? updateCatalog);

    IProductViewModel Create(IUpdateSession updateSession);
}