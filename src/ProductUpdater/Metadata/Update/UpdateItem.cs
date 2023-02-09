using System;
using AnakinRaW.AppUpaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpaterFramework.Metadata.Update;

public class UpdateItem : IUpdateItem
{
    public IInstallableComponent? InstalledComponent { get; }

    public IInstallableComponent? UpdateComponent { get; }

    public UpdateAction Action { get; }

    public UpdateItem(IInstallableComponent? installedComponent, IInstallableComponent? updateComponent, UpdateAction action)
    {
        if (installedComponent is null && updateComponent is null)
            throw new InvalidOperationException("Cannot create update item from no component information.");
        if (installedComponent is not null && updateComponent is not null &&
            !ProductComponentIdentityComparer.VersionAndBranchIndependent.Equals(installedComponent, updateComponent))
        {
            throw new InvalidOperationException(
                $"Cannot get action from not-matching product components {installedComponent.Id}:{updateComponent.Id}");
        }
        InstalledComponent = installedComponent;
        UpdateComponent = updateComponent;
        Action = action;
    }
}