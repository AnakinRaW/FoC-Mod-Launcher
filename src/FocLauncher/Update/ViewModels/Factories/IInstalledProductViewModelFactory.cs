using System;
using AnakinRaW.AppUpaterFramework.Catalog;
using AnakinRaW.ProductMetadata;

namespace FocLauncher.Update.ViewModels;

internal interface IInstalledProductViewModelFactory
{
    IInstalledProductViewModel Create(IInstalledProduct product, IUpdateCatalog? updateCatalog,
        IServiceProvider serviceProvider);
}