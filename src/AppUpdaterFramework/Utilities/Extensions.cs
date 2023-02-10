﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;

namespace AnakinRaW.AppUpaterFramework.Utilities;

internal static class Extensions
{
    internal static IEnumerable<IInstallableComponent> GetInstallableComponents(this IProductCatalog<IProductComponent> catalog)
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