using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Updater;

namespace AnakinRaW.AppUpdaterFramework.ViewModels.Factories;

internal interface IProductViewModelFactory
{
    IProductViewModel Create(IInstalledProduct product, IUpdateCatalog? updateCatalog);

    IProductViewModel Create(IUpdateSession updateSession);
}