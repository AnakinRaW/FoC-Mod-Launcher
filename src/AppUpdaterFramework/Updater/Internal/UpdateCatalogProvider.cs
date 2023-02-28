﻿using System;
using System.Collections.Generic;
using System.Linq;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.AppUpaterFramework.Product.Manifest;
using AnakinRaW.AppUpaterFramework.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Updater;

internal class UpdateCatalogProvider : IUpdateCatalogProvider
{
    private readonly IManifestInstallationDetector _detector;

    public UpdateCatalogProvider(IServiceProvider serviceProvider)
    {
        _detector = serviceProvider.GetRequiredService<IManifestInstallationDetector>();
    }

    public IUpdateCatalog Create(IInstalledProduct installedProduct, IInstalledComponentsCatalog currentCatalog, IProductManifest availableCatalog)
    {
        Requires.NotNull(currentCatalog, nameof(currentCatalog));
        Requires.NotNull(availableCatalog, nameof(availableCatalog));

        if (!ProductReferenceEqualityComparer.NameOnly.Equals(currentCatalog.Product, availableCatalog.Product))
            throw new InvalidOperationException("Cannot build update catalog from different products.");

        var currentInstalledComponents = new HashSet<IInstallableComponent>(currentCatalog.GetInstallableComponents(),
            ProductComponentIdentityComparer.VersionIndependent);

        var availableInstallableComponents = new HashSet<IInstallableComponent>(availableCatalog.GetInstallableComponents(),
            ProductComponentIdentityComparer.VersionIndependent);

        if (!currentInstalledComponents.Any() && !availableInstallableComponents.Any())
            return UpdateCatalog.CreateEmpty(installedProduct, availableCatalog.Product);

        // Empty available catalog: Uninstall
        if (!availableInstallableComponents.Any())
            return new UpdateCatalog(installedProduct, availableCatalog.Product, currentInstalledComponents
                .Select(c => new UpdateItem(c, null, UpdateAction.Delete)), UpdateCatalogAction.Uninstall);

        // Empty current catalog: Fresh install
        if (!currentInstalledComponents.Any())
            return new UpdateCatalog(installedProduct, availableCatalog.Product, availableInstallableComponents
                    .Select(c => new UpdateItem(null, c, UpdateAction.Update)), UpdateCatalogAction.Install);


        var availableInstalledComponents =
            _detector.DetectInstalledComponents(availableCatalog, installedProduct.Variables);


        var updateItems = Compare(currentCatalog, availableInstalledComponents);

        var action = updateItems.Any(i => i.Action is UpdateAction.Delete or UpdateAction.Update)
            ? UpdateCatalogAction.Update
            : UpdateCatalogAction.None;

        return new UpdateCatalog(installedProduct, availableCatalog.Product, updateItems, action);
    }


    private static ICollection<IUpdateItem> Compare(IInstalledComponentsCatalog currentCatalog, IEnumerable<IInstallableComponent> availableComponents)
    {
        var updateItems = new List<IUpdateItem>();

        var currentItems = currentCatalog.Items.ToList();

        foreach (var availableItem in availableComponents)
        {
            var installedComponent = currentItems.FirstOrDefault(c =>
                ProductComponentIdentityComparer.VersionAndBranchIndependent.Equals(c, availableItem));

            if (installedComponent is not null)
                currentItems.Remove(installedComponent);

            var action = availableItem.DetectedState == DetectionState.Present
                ? UpdateAction.Keep
                : UpdateAction.Update;
            updateItems.Add(new UpdateItem(installedComponent, availableItem, action));
        }

        foreach (var currentItem in currentItems)
            updateItems.Add(new UpdateItem(currentItem, null, UpdateAction.Delete));

        return updateItems;
    }
}