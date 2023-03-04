using System;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Update;

public sealed class UpdateItem : IUpdateItem
{
    public string ComponentId { get; }

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

        ComponentId = installedComponent?.Id ?? updateComponent!.GetUniqueId();
        InstalledComponent = installedComponent;
        UpdateComponent = updateComponent;
        Action = action;
    }

    public bool Equals(IUpdateItem other)
    {
        return ComponentId == other.ComponentId;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is UpdateItem other && Equals(other);
    }

    public override int GetHashCode()
    {
        return ComponentId.GetHashCode();
    }
}