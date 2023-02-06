using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.ProductMetadata.Component;

namespace AnakinRaW.ProductMetadata.Services;

public interface ICurrentInstallation
{
    IDirectoryInfo InstallLocation { get; }

    IReadOnlyCollection<IProductComponent> GetManifestComponents();

    IReadOnlyCollection<IProductComponent> FindInstalledComponents();
}