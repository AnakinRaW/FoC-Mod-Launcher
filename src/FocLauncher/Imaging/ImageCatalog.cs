using System;
using System.Collections.Generic;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.Imaging;

internal class ImageCatalog : ImmutableImageCatalog
{
    private static readonly Lazy<ImageCatalog> LazyConstruction = new(() => new ImageCatalog());
    public static ImageCatalog Instance => LazyConstruction.Value;

    public static ImageDefinition SettingsDefinition => new()
    {
        Kind = ImageFileKind.Xaml,
        Moniker = Monikers.Settings,
        Source = ResourcesUriCreator.Create("Settings_16x", ImageFileKind.Xaml),
        CanTheme = true
    };

    public static ImageDefinition UndoDefinition => new()
    {
        Kind = ImageFileKind.Png,
        Moniker = Monikers.Undo,
        Source = ResourcesUriCreator.Create("Undo_16x", ImageFileKind.Png),
        CanTheme = true
    };

    public static ImageDefinition GithubDefinition => new()
    {
        Kind = ImageFileKind.Png,
        Moniker = Monikers.Github,
        Source = ResourcesUriCreator.Create("GitHub_Mark_32px", ImageFileKind.Png),
        CanTheme = true
    };




    public static IEnumerable<ImageDefinition> Definitions = new List<ImageDefinition>
    {
        SettingsDefinition,
        UndoDefinition
    };

    private ImageCatalog() : base(Definitions)
    {
    }
}