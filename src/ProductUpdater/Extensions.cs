using System;
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

    public static IEnumerable<IInstallableComponent> GetInstallableComponentsRecursive(this IProductCatalog catalog)
    {
        if (!catalog.Items.Any())
            return Enumerable.Empty<IInstallableComponent>();
        var result = new List<IInstallableComponent>();
        foreach (var item in catalog.Items) 
            AddInstallableComponentsRecursive(item, result);
        return result;
    }

    public static void AddInstallableComponentsRecursive(IProductComponent component, ICollection<IInstallableComponent> result)
    {
        switch (component)
        {
            case IComponentGroup group:
            {
                foreach (var groupComponent in group.Components)
                    AddInstallableComponentsRecursive(groupComponent, result);
                break;
            }
            case IInstallableComponent installable:
                result.Add(installable);
                break;
            default:
                throw new NotSupportedException("Unknown component type");
        }
    }
}