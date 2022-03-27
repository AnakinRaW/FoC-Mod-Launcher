using System.IO.Abstractions;
using Sklavenwalker.ProductMetadata.Manifest;

namespace Sklavenwalker.ProductMetadata.Services;

public interface IAvailableManifestBuilder
{
    IManifest Build(IFileInfo manifestFile, IProductReference product);
}