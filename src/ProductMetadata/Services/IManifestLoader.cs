﻿using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.ProductMetadata.Catalog;

namespace AnakinRaW.ProductMetadata.Services;

public interface IManifestLoader
{
    Task<IProductManifest> LoadManifest(IFileInfo manifestFile, IProductReference productReference, CancellationToken cancellationToken);

    Task<IProductManifest> LoadManifest(Stream manifest, IProductReference productReference, CancellationToken cancellationToken, bool keepOpen = false);
}