using System.Collections.Generic;
using Sklavenwalker.ProductMetadata.Component;
using Sklavenwalker.ProductUpdater.Catalog;

namespace Sklavenwalker.ProductUpdater;

public interface IComponentComparer
{
    public UpdateAction Compare(
        IInstallableComponent? installableComponent, 
        IInstallableComponent availableComponent,
        IDictionary<string, string?>? properties = null);
}