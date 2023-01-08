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
        ImakgeKey = ImageKeys.Settings,
        Source = ResourcesUriCreator.Create("Settings_16x", ImageFileKind.Xaml),
        CanTheme = true
    };

    public static ImageDefinition TrooperDefinition => new()
    {
        Kind = ImageFileKind.Png,
        ImakgeKey = ImageKeys.Trooper,
        Source = ResourcesUriCreator.Create("sadTrooper", ImageFileKind.Png),
        CanTheme = false
    };

    public static ImageDefinition UndoDefinition => new()
    {
        Kind = ImageFileKind.Png,
        ImakgeKey = ImageKeys.Undo,
        Source = ResourcesUriCreator.Create("Undo_16x", ImageFileKind.Png),
        CanTheme = true
    };

    public static ImageDefinition GithubDefinition => new()
    {
        Kind = ImageFileKind.Png,
        ImakgeKey = ImageKeys.Github,
        Source = ResourcesUriCreator.Create("GitHub_Mark_32px", ImageFileKind.Png),
        CanTheme = true
    };

    public static ImageDefinition UpdateIconDefinition => new()
    {
        Kind = ImageFileKind.Xaml,
        ImakgeKey = ImageKeys.UpdateIcon,
        Source = ResourcesUriCreator.Create("StatusUpdateAvailable", ImageFileKind.Xaml),
        CanTheme = true
    };

    public static ImageDefinition HelpIconDefinition => new()
    {
        Kind = ImageFileKind.Xaml,
        ImakgeKey = ImageKeys.StatusHelpIcon,
        Source = ResourcesUriCreator.Create("StatusHelp", ImageFileKind.Xaml),
        CanTheme = true
    };

    public static ImageDefinition AppIconDefinition => new()
    {
        Kind = ImageFileKind.Png,
        ImakgeKey = ImageKeys.AppIcon,
        Source = ResourcesUriCreator.Create("AppIcon", ImageFileKind.Png),
        CanTheme = false
    };




    public static IEnumerable<ImageDefinition> Definitions = new List<ImageDefinition>
    {
        AppIconDefinition,
        SettingsDefinition,
        UndoDefinition,
        GithubDefinition, 
        TrooperDefinition,
        UpdateIconDefinition,
        HelpIconDefinition
    };

    private ImageCatalog() : base(Definitions)
    {
    }
}