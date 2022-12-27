using System;
using System.Collections;
using System.Collections.Generic;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging;

public abstract class ImmutableImageCatalog : IImageCatalog
{
    private readonly Dictionary<string, ImageDefinition> _imageDefinitions = new();

    public Type CatalogType => GetType();

    protected ImmutableImageCatalog(IEnumerable<ImageDefinition> definitions)
    {
        foreach (var imageDefinition in definitions)
        {
            if (imageDefinition.ImakgeKey.CatalogType != CatalogType)
                throw new ArgumentException(
                    "definitions contains an ImageDefinition which does not belong to this catalog.");
            _imageDefinitions[imageDefinition.ImakgeKey.Name] = imageDefinition;
        }
    }

    public bool GetDefinition(string imageId, out ImageDefinition imageDefinition)
    {
        return _imageDefinitions.TryGetValue(imageId, out imageDefinition);
    }

    public IEnumerator<ImageDefinition> GetEnumerator()
    {
        return _imageDefinitions.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}