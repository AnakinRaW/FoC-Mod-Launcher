using System;
using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace AnakinRaW.AppUpdaterFramework.Imaging;

internal class ImageCatalog : ImmutableImageCatalog
{
    private static readonly Lazy<ImageCatalog> LazyConstruction = new(() => new ImageCatalog());
    public static ImageCatalog Instance => LazyConstruction.Value;
    
    public static ImageDefinition StatusErrorDefinition => new()
    {
        Kind = ImageFileKind.Xaml,
        ImakgeKey = ImageKeys.StatusError,
        Source = ResourcesUriCreator.Create("StatusError", ImageFileKind.Xaml),
        CanTheme = true
    };
    
    public static ImageDefinition StatusOkDefinition => new()
    {
        Kind = ImageFileKind.Xaml,
        ImakgeKey = ImageKeys.StatusOK,
        Source = ResourcesUriCreator.Create("StatusOK", ImageFileKind.Xaml),
        CanTheme = true
    };


    public static readonly IEnumerable<ImageDefinition> Definitions = new List<ImageDefinition>
    {
        StatusErrorDefinition,
        StatusOkDefinition,
    };

    private ImageCatalog() : base(Definitions)
    {
    }
}