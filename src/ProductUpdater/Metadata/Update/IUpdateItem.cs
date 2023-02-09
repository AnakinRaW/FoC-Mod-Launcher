using AnakinRaW.AppUpaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpaterFramework.Metadata.Update;

public interface IUpdateItem
{
    IInstallableComponent? InstalledComponent { get; }

    IInstallableComponent? UpdateComponent { get; }

    UpdateAction Action { get; }
}