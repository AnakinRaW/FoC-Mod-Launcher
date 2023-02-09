﻿using System;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Metadata.Update;

namespace FocLauncher.Update.ViewModels;

internal interface IInstalledProductViewModelFactory
{
    IInstalledProductViewModel Create(IInstalledProduct product, IUpdateCatalog? updateCatalog,
        IServiceProvider serviceProvider);
}