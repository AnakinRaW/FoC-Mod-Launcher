using System;
using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Wpf.Imaging;

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

    public static ImageDefinition UACShieldDefinition => new()
    {
        Kind = ImageFileKind.Xaml,
        ImakgeKey = ImageKeys.UACShield,
        Source = ResourcesUriCreator.Create("UacShield", ImageFileKind.Xaml),
        CanTheme = true
    };

    public static ImageDefinition TrooperDefinition => new()
    {
        Kind = ImageFileKind.Png,
        ImakgeKey = ImageKeys.Trooper,
        Source = ResourcesUriCreator.Create("sadTrooper", ImageFileKind.Png),
        CanTheme = false
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

    public static ImageDefinition VaderDefinition => new()
    {
        Kind = ImageFileKind.Png,
        ImakgeKey = ImageKeys.Vader,
        Source = ResourcesUriCreator.Create("vader", ImageFileKind.Png),
        CanTheme = false
    };

    public static ImageDefinition PalpatineDefinition => new()
    {
        Kind = ImageFileKind.Png,
        ImakgeKey = ImageKeys.Palpatine,
        Source = ResourcesUriCreator.Create("senat", ImageFileKind.Png),
        CanTheme = false
    };

    public static ImageDefinition SwPulpDefinition => new()
    {
        Kind = ImageFileKind.Png,
        ImakgeKey = ImageKeys.SwPulp,
        Source = ResourcesUriCreator.Create("kill", ImageFileKind.Png),
        CanTheme = false
    };


    public static readonly IEnumerable<ImageDefinition> Definitions = new List<ImageDefinition>
    {
        AppIconDefinition,
        UACShieldDefinition,
        SettingsDefinition,
        GithubDefinition, 
        TrooperDefinition,
        UpdateIconDefinition,
        HelpIconDefinition,
        StatusErrorDefinition,
        StatusOkDefinition,
        VaderDefinition,
        PalpatineDefinition, 
        SwPulpDefinition
    };

    private ImageCatalog() : base(Definitions)
    {
    }
}