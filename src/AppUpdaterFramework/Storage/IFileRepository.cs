using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Storage;

internal interface IFileRepository
{
    IFileInfo AddComponent(IInstallableComponent component);

    IFileInfo? GetComponent(IInstallableComponent component);

    IDictionary<IInstallableComponent, IFileInfo> GetComponents();

    ISet<IFileInfo> GetFiles();

    void Clear();
}