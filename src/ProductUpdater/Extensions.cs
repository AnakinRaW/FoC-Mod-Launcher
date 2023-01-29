using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnakinRaW.ProductMetadata.Catalog;
using AnakinRaW.ProductMetadata.Component;
using AnakinRaW.ProductUpdater.Catalog;
using Validation;

namespace AnakinRaW.ProductUpdater;

public static class Extensions
{
    public static bool RequiresUpdate(this IUpdateCatalog updateCatalog)
    {
        Requires.NotNull(updateCatalog, nameof(updateCatalog));
        return updateCatalog.UpdateItems.Any() && updateCatalog.UpdateItems.Any(i => i.Action != UpdateAction.Update);
    }

    internal static IEnumerable<IInstallableComponent> GetInstallableComponents(this IProductCatalog catalog)
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

    internal static void RaiseAsync(this EventHandler? handler, object sender, EventArgs e)
    {
        Task.Run(() => handler?.Invoke(sender, e));
    }

    internal static void RaiseAsync<T>(this EventHandler<T>? handler, object sender, T e)
    {
        Task.Run(() => handler?.Invoke(sender, e));
    }
}