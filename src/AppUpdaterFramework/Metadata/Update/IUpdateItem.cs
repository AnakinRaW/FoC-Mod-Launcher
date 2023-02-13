using System;
using AnakinRaW.AppUpaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpaterFramework.Metadata.Update;

public interface IUpdateItem : IEquatable<IUpdateItem>
{
    string ComponentId { get; }

    IInstallableComponent? InstalledComponent { get; }

    IInstallableComponent? UpdateComponent { get; }

    UpdateAction Action { get; }
}