using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Media;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging;

public class ImageLibrary
{
    public static readonly ImageMoniker InvalidImageMoniker = default;
    public static readonly Color DefaultGrayscaleBiasColor = Color.FromArgb(64, byte.MaxValue, byte.MaxValue, byte.MaxValue);
    public static readonly Color HighContrastGrayscaleBiasColor = Color.FromArgb(192, byte.MaxValue, byte.MaxValue, byte.MaxValue);

    private static readonly Lazy<ImageLibrary> LazyConstruction = new(() => new ImageLibrary());

    private readonly HashSet<IImageCatalog> _imageCatalogs = new(new ImageCatalogEqualityComparer());

    private CustomImageCatalog CustomImageCatalog { get; } = new();

    public static ImageLibrary Instance => LazyConstruction.Value;

    private ImageLibrary()
    {
    }

    public void LoadCatalog(IImageCatalog catalog)
    {
        if (!_imageCatalogs.Add(catalog))
            throw new InvalidOperationException($"Catalog with id {catalog.CatalogType} already exists.");
    }

    internal ImageSource? GetImage(ImageMoniker moniker, ImageAttributes attributes)
    {
        return null;
    }

    public ImageDefinition AddCustomImage(ImageSource inputImage, bool canTheme)
    {
        return default;
    }
}

internal class ImageCatalogEqualityComparer : EqualityComparer<IImageCatalog>
{
    public override bool Equals(IImageCatalog x, IImageCatalog y)
    {
        return x.CatalogType == y.CatalogType;
    }

    public override int GetHashCode(IImageCatalog obj)
    {
        return obj.CatalogType.GetHashCode();
    }
}

public interface IImageCatalog : IEnumerable<ImageDefinition>
{
    public Type CatalogType { get; }
}

public struct ImageDefinition
{
    public ImageMoniker Moniker;
    public Uri Source;
    public ImageFileKind Kind;
    public bool CanTheme;
}

public enum ImageFileKind
{
    Xaml,
    Png
}

internal class CustomImageCatalog : IImageCatalog
{
    public Type CatalogType => GetType();

    private readonly HashSet<ImageDefinition> _definitions = new();

    public bool AddDefinition(ImageDefinition definition)
    {
        return _definitions.Add(definition);
    }

    public IEnumerator<ImageDefinition> GetEnumerator()
    {
        return _definitions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public abstract class ImmutableImageCatalog : IImageCatalog
{
    private readonly ImmutableHashSet<ImageDefinition> _definitions;
    
    public Type CatalogType => GetType();

    protected ImmutableImageCatalog(IEnumerable<ImageDefinition> definitions)
    {
        _definitions = ImmutableHashSet.Create(definitions.ToArray());
    }

    public IEnumerator<ImageDefinition> GetEnumerator()
    {
        return _definitions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}