using System.Collections.Generic;
using System.Linq;
using Sklavenwalker.ProductMetadata.Catalog;
using Sklavenwalker.ProductMetadata.Component;
using Sklavenwalker.ProductUpdater.Catalog;
using Validation;

namespace Sklavenwalker.ProductUpdater;

internal static class Extensions
{
    public static bool RequiresUpdate(this IUpdateCatalog updateCatalog)
    {
        Requires.NotNull(updateCatalog, nameof(updateCatalog));
        return updateCatalog.UpdateItems.Any() && updateCatalog.UpdateItems.Any(i => i.Action != UpdateAction.Update);
    }

    public static IEnumerable<IInstallableComponent> GetInstallableComponents(this IProductCatalog catalog)
    {
        if (!catalog.Items.Any())
            return Enumerable.Empty<IInstallableComponent>();
        var result = new List<IInstallableComponent>();
        foreach (var item in catalog.Items)
        {
            if (item is IInstallableComponent installable)
                result.Add(installable);
        }
        return result;
    }
}