using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Component;
using AnakinRaW.ProductUpdater.Catalog;

namespace AnakinRaW.ProductUpdater.Services;

internal interface IComponentComparer
{
    public UpdateAction Compare(
        IInstallableComponent? installableComponent,
        IInstallableComponent availableComponent,
        IDictionary<string, string?>? properties = null);
}