using AnakinRaW.ProductMetadata.Component;

namespace AnakinRaW.ProductUpdater.Catalog;

public interface IUpdateItem
{
    IInstallableComponent? InstalledComponent { get; }

    IInstallableComponent? UpdateComponent { get; }

    UpdateAction Action { get; }
}