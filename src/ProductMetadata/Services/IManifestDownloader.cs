﻿using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace AnakinRaW.ProductMetadata.Services;

public interface IManifestDownloader
{
    Task<IFileInfo> GetManifest(Uri manifestPath, CancellationToken token = default);
}