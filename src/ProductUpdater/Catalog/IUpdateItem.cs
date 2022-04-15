using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductUpdater.Catalog;

public interface IUpdateItem
{
    IInstallableComponent? InstalledComponent { get; }

    IInstallableComponent? UpdateComponent { get; }

    UpdateAction Action { get; }
}