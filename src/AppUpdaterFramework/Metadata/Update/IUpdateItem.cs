using System;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Update;

public interface IUpdateItem : IEquatable<IUpdateItem>
{
    string ComponentId { get; }

    IInstallableComponent? InstalledComponent { get; }

    IInstallableComponent? UpdateComponent { get; }

    UpdateAction Action { get; }
}