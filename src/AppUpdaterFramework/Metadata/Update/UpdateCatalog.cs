using System.Collections.Generic;
using System.Linq;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Update;

public class UpdateCatalog : IUpdateCatalog
{
    public IInstalledProduct InstalledProduct { get; }

    public IProductReference UpdateReference { get; }

    public IReadOnlyCollection<IUpdateItem> UpdateItems { get; }

    public UpdateCatalogAction Action { get; }

    public UpdateCatalog(IInstalledProduct installedProduct, IProductReference updateReference, IEnumerable<IUpdateItem> updateItems, UpdateCatalogAction action = UpdateCatalogAction.Update)
    {
        Requires.NotNull(installedProduct, nameof(installedProduct));
        Requires.NotNull(updateReference, nameof(updateReference));
        InstalledProduct = installedProduct;
        UpdateReference = updateReference;
        UpdateItems = updateItems.ToList();
        Action = action;
    }

    internal static UpdateCatalog CreateEmpty(IInstalledProduct installedProduct, IProductReference updateReference)
    {
        return new UpdateCatalog(installedProduct, updateReference, Enumerable.Empty<IUpdateItem>(), UpdateCatalogAction.None);
    }
}