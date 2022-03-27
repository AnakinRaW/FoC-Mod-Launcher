using System;
using System.IO.Abstractions;

namespace Sklavenwalker.ProductMetadata.Services;

public interface IManifestFileResolver
{
    IFileInfo GetManifest(Uri manifestPath);
}