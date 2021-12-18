using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging;

public class ImageLibrary
{
    public static readonly ImageMoniker InvalidImageMoniker = default;

    public static readonly Color DefaultGrayscaleBiasColor =
        Color.FromArgb(64, byte.MaxValue, byte.MaxValue, byte.MaxValue);

    public static readonly Color HighContrastGrayscaleBiasColor =
        Color.FromArgb(192, byte.MaxValue, byte.MaxValue, byte.MaxValue);

    private static readonly Lazy<ImageLibrary> LazyConstruction = new(() => new ImageLibrary());

    private readonly HashSet<IImageCatalog> _imageCatalogs = new(new ImageCatalogEqualityComparer());

    private readonly Dictionary<(ImageMoniker, ImageAttributes), BitmapSource?> _imageCache = new();

    private CustomImageCatalog CustomImageCatalog { get; } = new();

    public static ImageLibrary Instance => LazyConstruction.Value;

    private ImageLibrary()
    {
        _imageCatalogs.Add(CustomImageCatalog);
    }

    public void LoadCatalog(IImageCatalog catalog)
    {
        if (!_imageCatalogs.Add(catalog))
            throw new InvalidOperationException($"Catalog with id {catalog.CatalogType} already exists.");
    }

    public ImageMoniker AddCustomImage(ImageSource inputImage, bool canTheme)
    {
        Requires.NotNull(inputImage, nameof(inputImage));
        try
        {
            if (!inputImage.IsFrozen)
                inputImage = (ImageSource)inputImage.GetAsFrozen();
        }
        catch (InvalidOperationException ex)
        {
            throw new ArgumentException("The only supported ImageSource types are BitmapSource and DrawingImage.",
                nameof(inputImage), ex);
        }

        if (inputImage is not BitmapFrame bitmap)
            return default; 
        
        var uriString = bitmap.Decoder?.ToString();
        if (string.IsNullOrEmpty(uriString) || !Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var imageUri))
            return default;

        var moniker = BuildCustomMonikerFromUri(imageUri, out var kind);

        if (moniker == InvalidImageMoniker)
            return default;

        if (CustomImageCatalog.GetDefinition(moniker.Name, out _))
            return moniker;

        var def = new ImageDefinition
        {
            CanTheme = canTheme,
            Moniker = moniker,
            Kind = kind, 
            Source = imageUri
        };

        CustomImageCatalog.AddDefinition(def);
        return moniker;
    }

    private static ImageMoniker BuildCustomMonikerFromUri(Uri source, out ImageFileKind kind)
    {
        kind = ImageFileKind.Xaml;
        var localPath = source.LocalPath;
        var extension = Path.GetExtension(localPath);
        if (string.IsNullOrEmpty(extension))
            return default;
        kind = extension.ToLowerInvariant() switch
        {
            "xaml" => ImageFileKind.Xaml,
            "png" => ImageFileKind.Png,
            _ => kind
        };
        return new ImageMoniker { CatalogType = typeof(CustomImageCatalog), Name = localPath };
    }

    public ImageSource? GetImage(ImageMoniker moniker, ImageAttributes attributes)
    {
        if (moniker == InvalidImageMoniker)
            return null;

        if (_imageCache.TryGetValue((moniker, attributes), out var cachedImage))
            return cachedImage;

        var catalog = _imageCatalogs.FirstOrDefault(x => x.CatalogType == moniker.CatalogType);
        if (catalog == null)
            return null;
        if (!catalog.GetDefinition(moniker.Name, out var imageDefinition))
            return null;
        ImagingUtilities.ValidateAttributes(attributes);

        try
        {
            var image = ImagingUtilities.LoadImage(attributes, imageDefinition);
            _imageCache[(moniker, attributes)] = image;
            return image;
        }
        catch (Exception)
        {
            return null;
        }
    }
}