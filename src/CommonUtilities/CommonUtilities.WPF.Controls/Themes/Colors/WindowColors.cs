using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.Themes.Colors;

public static class WindowColors
{
    public static ComponentResourceKey MainWindowInactiveCaption { get; } = new(typeof(WindowColors), nameof(MainWindowInactiveCaption));
    public static ComponentResourceKey MainWindowInactiveCaptionText { get; } = new(typeof(WindowColors), nameof(MainWindowInactiveCaptionText));
    
    public static ComponentResourceKey EnvironmentBackground { get; } = new(typeof(WindowColors), nameof(EnvironmentBackground));
    
    public static ComponentResourceKey MainWindowActiveDefaultBorder { get; } = new(typeof(WindowColors), nameof(MainWindowActiveDefaultBorder));
    public static ComponentResourceKey MainWindowInactiveBorder { get; } = new(typeof(WindowColors), nameof(MainWindowInactiveBorder));
    public static ComponentResourceKey MainWindowActiveCaption { get; } = new(typeof(WindowColors), nameof(MainWindowActiveCaption));
    public static ComponentResourceKey MainWindowActiveCaptionText { get; } = new(typeof(WindowColors), nameof(MainWindowActiveCaptionText));
    public static ComponentResourceKey MainWindowButtonActiveBorder { get; } = new(typeof(WindowColors), nameof(MainWindowButtonActiveBorder));
    public static ComponentResourceKey MainWindowButtonActiveGlyph { get; } = new(typeof(WindowColors), nameof(MainWindowButtonActiveGlyph));
    public static ComponentResourceKey MainWindowButtonHoverActive { get; } = new(typeof(WindowColors), nameof(MainWindowButtonHoverActive));
    public static ComponentResourceKey MainWindowButtonHoverActiveBorder { get; } = new(typeof(WindowColors), nameof(MainWindowButtonHoverActiveBorder));
    public static ComponentResourceKey MainWindowButtonHoverActiveGlyph { get; } = new(typeof(WindowColors), nameof(MainWindowButtonHoverActiveGlyph));
    public static ComponentResourceKey MainWindowButtonDown { get; } = new(typeof(WindowColors), nameof(MainWindowButtonDown));
    public static ComponentResourceKey MainWindowButtonDownBorder { get; } = new(typeof(WindowColors), nameof(MainWindowButtonDownBorder));
    public static ComponentResourceKey MainWindowButtonDownGlyph { get; } = new(typeof(WindowColors), nameof(MainWindowButtonDownGlyph));
    public static ComponentResourceKey MainWindowButtonInactiveBorder { get; } = new(typeof(WindowColors), nameof(MainWindowButtonInactiveBorder));
    public static ComponentResourceKey MainWindowButtonInactiveGlyph { get; } = new(typeof(WindowColors), nameof(MainWindowButtonInactiveGlyph));
    public static ComponentResourceKey MainWindowButtonHoverInactive { get; } = new(typeof(WindowColors), nameof(MainWindowButtonHoverInactive));
    public static ComponentResourceKey MainWindowButtonHoverInactiveBorder { get; } = new(typeof(WindowColors), nameof(MainWindowButtonHoverInactiveBorder));
    public static ComponentResourceKey MainWindowButtonHoverInactiveGlyph { get; } = new(typeof(WindowColors), nameof(MainWindowButtonHoverInactiveGlyph));
    public static ComponentResourceKey MainWindowInactiveIconDefault { get; } = new(typeof(WindowColors), nameof(MainWindowInactiveIconDefault));
    public static ComponentResourceKey MainWindowActiveIconDefault { get; } = new(typeof(WindowColors), nameof(MainWindowActiveIconDefault));
    public static ComponentResourceKey MainWindowResizeGripTexture1 { get; } = new(typeof(WindowColors), nameof(MainWindowResizeGripTexture1));
    public static ComponentResourceKey MainWindowResizeGripTexture2 { get; } = new(typeof(WindowColors), nameof(MainWindowResizeGripTexture2));
    
    public static ComponentResourceKey DialogBackground { get; } = new(typeof(WindowColors), nameof(DialogBackground));
    public static ComponentResourceKey DialogContentBackground { get; } = new(typeof(WindowColors), nameof(DialogContentBackground));
    public static ComponentResourceKey DialogContentText { get; } = new(typeof(WindowColors), nameof(DialogContentText));

    public static ComponentResourceKey StatusBarDefault { get; } = new(typeof(WindowColors), nameof(StatusBarDefault));
    public static ComponentResourceKey StatusBarDefaultText { get; } = new(typeof(WindowColors), nameof(StatusBarDefaultText));
}