using System;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductUpdater.Catalog;

namespace FocLauncher.Update.ViewModels;

internal interface IInstalledProductViewModelFactory
{
    IInstalledProductViewModel Create(IInstalledProduct product, IUpdateCatalog? updateCatalog,
        IServiceProvider serviceProvider);
}