﻿using System;
using System.Collections.Generic;
using System.Linq;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductMetadata.Catalog;
using AnakinRaW.ProductMetadata.Component;
using AnakinRaW.ProductUpdater.Catalog;
using Validation;

namespace AnakinRaW.ProductUpdater.Services;

internal class UpdateCatalogBuilder : IUpdateCatalogBuilder
{
    private readonly IComponentComparer _comparer;

    public UpdateCatalogBuilder(IServiceProvider serviceProvider)
    {
        _comparer = new ComponentUpdateComparer(serviceProvider);
    }

    internal UpdateCatalogBuilder(IComponentComparer comparer)
    {
        _comparer = comparer;
    }

    public IUpdateCatalog Build(IInstalledProductCatalog installedCatalog, IProductManifest availableCatalog)
    {
        Requires.NotNull(installedCatalog, nameof(installedCatalog));
        Requires.NotNull(availableCatalog, nameof(availableCatalog));

        if (!ProductReferenceEqualityComparer.Default.Equals(installedCatalog.Product, availableCatalog.Product))
            throw new InvalidOperationException("Cannot build update catalog from different products.");

        var currentInstalledComponents = new HashSet<IInstallableComponent>(installedCatalog.GetInstallableComponents(),
            ProductComponentIdentityComparer.VersionIndependent);

        var availableInstallableComponents = new HashSet<IInstallableComponent>(availableCatalog.GetInstallableComponents(),
            ProductComponentIdentityComparer.VersionIndependent);

        if (!currentInstalledComponents.Any() && !availableInstallableComponents.Any())
            return UpdateCatalog.CreateEmpty(availableCatalog.Product);

        // Empty available catalog: Uninstall
        if (!availableInstallableComponents.Any())
            return new UpdateCatalog(availableCatalog.Product, currentInstalledComponents.Select(c => new UpdateItem(c, null, UpdateAction.Delete)));

        // Empty current catalog: Fresh install
        if (!currentInstalledComponents.Any())
            return new UpdateCatalog(availableCatalog.Product, availableInstallableComponents.Select(c => new UpdateItem(null, c, UpdateAction.Update)));

        var updateItems = new List<IUpdateItem>();
        foreach (var availableItem in availableInstallableComponents)
        {
            if (availableItem.OriginInfo is null)
                throw new CatalogException("Available catalog component must have origin data information.");

            var installedComponent = currentInstalledComponents.FirstOrDefault(c =>
                ProductComponentIdentityComparer.VersionAndBranchIndependent.Equals(c, availableItem));

            var action = _comparer.Compare(installedComponent, availableItem, installedCatalog.Product.ProductVariables.ToDictionary());
            var item = new UpdateItem(installedComponent, availableItem, action);
            updateItems.Add(item);
        }

        // Remove components not found in the requested catalog
        foreach (var currentItem in currentInstalledComponents)
        {
            var item = new UpdateItem(currentItem, null, UpdateAction.Delete);
            updateItems.Add(item);
        }

        return new UpdateCatalog(availableCatalog.Product, updateItems);
    }
}