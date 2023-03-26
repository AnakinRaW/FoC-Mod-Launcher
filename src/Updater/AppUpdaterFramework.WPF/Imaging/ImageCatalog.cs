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
        ImakgeKey = UpdaterImageKeys.StatusError,
        Source = ResourcesUriCreator.Create("StatusError", ImageFileKind.Xaml),
        CanTheme = true
    };
    
    public static ImageDefinition StatusOkDefinition => new()
    {
        Kind = ImageFileKind.Xaml,
        ImakgeKey = UpdaterImageKeys.StatusOK,
        Source = ResourcesUriCreator.Create("StatusOK", ImageFileKind.Xaml),
        CanTheme = true
    };

    public static ImageDefinition UACShieldDefinition => new()
    {
        Kind = ImageFileKind.Xaml,
        ImakgeKey = UpdaterImageKeys.UACShield,
        Source = ResourcesUriCreator.Create("UacShield", ImageFileKind.Xaml),
        CanTheme = true
    };


    public static readonly IEnumerable<ImageDefinition> Definitions = new List<ImageDefinition>
    {
        StatusErrorDefinition,
        StatusOkDefinition,
        UACShieldDefinition
    };

    private ImageCatalog() : base(Definitions)
    {
    }
}