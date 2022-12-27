using System;
using System.Collections;
using System.Collections.Generic;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging;

internal class CustomImageCatalog : IImageCatalog
{
    public Type CatalogType => GetType();

    private readonly Dictionary<string, ImageDefinition> _definitions = new();

    public bool GetDefinition(string imageId, out ImageDefinition imageDefinition)
    {
        return _definitions.TryGetValue(imageId, out imageDefinition);
    }

    public bool AddDefinition(ImageDefinition definition)
    {
        if (definition.ImakgeKey.CatalogType != CatalogType)
            throw new ArgumentException("ImageDefinition does not match this catalog.");
        if (_definitions.ContainsKey(definition.ImakgeKey.Name))
            return false;
        _definitions[definition.ImakgeKey.Name] = definition;
        return true;
    }

    public IEnumerator<ImageDefinition> GetEnumerator()
    {
        return _definitions.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}