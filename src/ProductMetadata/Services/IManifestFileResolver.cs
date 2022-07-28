﻿using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Sklavenwalker.ProductMetadata.Services;

public interface IManifestFileResolver
{
    Task<IFileInfo> GetManifest(Uri manifestPath, CancellationToken token = default);
}