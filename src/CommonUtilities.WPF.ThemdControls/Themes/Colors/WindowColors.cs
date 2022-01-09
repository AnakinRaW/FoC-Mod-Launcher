using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Themes.Colors;

public static class WindowColors
{
    private static ComponentResourceKey? _environmentBackground;
    private static ComponentResourceKey? _mainWindowActiveDefaultBorder;
    private static ComponentResourceKey? _mainWindowInactiveBorder;
    private static ComponentResourceKey? _mainWindowActiveCaption;
    private static ComponentResourceKey? _mainWindowInactiveCaption;
    private static ComponentResourceKey? _mainWindowActiveCaptionText;
    private static ComponentResourceKey? _mainWindowInactiveCaptionText;

    private static ComponentResourceKey? _mainWindowButtonActiveBorder;
    private static ComponentResourceKey? _mainWindowButtonActiveGlyph;
    private static ComponentResourceKey? _mainWindowButtonHoverActive;
    private static ComponentResourceKey? _mainWindowButtonHoverActiveBorder;
    private static ComponentResourceKey? _mainWindowButtonHoverActiveGlyph;
    private static ComponentResourceKey? _mainWindowButtonDown;
    private static ComponentResourceKey? _mainWindowButtonDownBorder;
    private static ComponentResourceKey? _mainWindowButtonDownGlyph;
    private static ComponentResourceKey? _mainWindowButtonInactiveBorder;
    private static ComponentResourceKey? _mainWindowButtonInactiveGlyph;
    private static ComponentResourceKey? _mainWindowButtonHoverInactive;
    private static ComponentResourceKey? _mainWindowButtonHoverInactiveBorder;
    private static ComponentResourceKey? _mainWindowButtonHoverInactiveGlyph;


    private static ComponentResourceKey? _mainWindowActiveIconDefault;
    private static ComponentResourceKey? _mainWindowInactiveIconDefault;

    private static ComponentResourceKey? _mainWindowResizeGripTexture1;
    private static ComponentResourceKey? _mainWindowResizeGripTexture2;

    private static ComponentResourceKey? _dialogBackground;
    private static ComponentResourceKey? _dialogContentBackground;
    private static ComponentResourceKey? _dialogContentText;

    private static ComponentResourceKey? _statusBarDefault;


    public static ComponentResourceKey MainWindowInactiveCaption => _mainWindowInactiveCaption ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowInactiveCaption));

    public static ComponentResourceKey MainWindowInactiveCaptionText => _mainWindowInactiveCaptionText ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowInactiveCaptionText));

    public static ComponentResourceKey EnvironmentBackground => _environmentBackground ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(EnvironmentBackground));

    public static ComponentResourceKey MainWindowActiveDefaultBorder => _mainWindowActiveDefaultBorder ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowActiveDefaultBorder));

    public static ComponentResourceKey MainWindowInactiveBorder => _mainWindowInactiveBorder ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowInactiveBorder));

    public static ComponentResourceKey MainWindowActiveCaption => _mainWindowActiveCaption ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowActiveCaption));

    public static ComponentResourceKey MainWindowActiveCaptionText => _mainWindowActiveCaptionText ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowActiveCaptionText));

    public static ComponentResourceKey MainWindowButtonActiveBorder => _mainWindowButtonActiveBorder ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowButtonActiveBorder));

    public static ComponentResourceKey MainWindowButtonActiveGlyph => _mainWindowButtonActiveGlyph ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowButtonActiveGlyph));

    public static ComponentResourceKey MainWindowButtonHoverActive => _mainWindowButtonHoverActive ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowButtonHoverActive));

    public static ComponentResourceKey MainWindowButtonHoverActiveBorder => _mainWindowButtonHoverActiveBorder ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowButtonHoverActiveBorder));

    public static ComponentResourceKey MainWindowButtonHoverActiveGlyph => _mainWindowButtonHoverActiveGlyph ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowButtonHoverActiveGlyph));

    public static ComponentResourceKey MainWindowButtonDown => _mainWindowButtonDown ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowButtonDown));

    public static ComponentResourceKey MainWindowButtonDownBorder => _mainWindowButtonDownBorder ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowButtonDownBorder));

    public static ComponentResourceKey MainWindowButtonDownGlyph => _mainWindowButtonDownGlyph ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowButtonDownGlyph));

    public static ComponentResourceKey MainWindowButtonInactiveBorder => _mainWindowButtonInactiveBorder ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowButtonInactiveBorder));

    public static ComponentResourceKey MainWindowButtonInactiveGlyph => _mainWindowButtonInactiveGlyph ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowButtonInactiveGlyph));

    public static ComponentResourceKey MainWindowButtonHoverInactive => _mainWindowButtonHoverInactive ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowButtonHoverInactive));

    public static ComponentResourceKey MainWindowButtonHoverInactiveBorder => _mainWindowButtonHoverInactiveBorder ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowButtonHoverInactiveBorder));

    public static ComponentResourceKey MainWindowButtonHoverInactiveGlyph => _mainWindowButtonHoverInactiveGlyph ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowButtonHoverInactiveGlyph));

    public static ComponentResourceKey MainWindowInactiveIconDefault => _mainWindowInactiveIconDefault ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowInactiveIconDefault));

    public static ComponentResourceKey MainWindowActiveIconDefault => _mainWindowActiveIconDefault ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowActiveIconDefault));

    public static ComponentResourceKey MainWindowResizeGripTexture1 => _mainWindowResizeGripTexture1 ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowResizeGripTexture1));

    public static ComponentResourceKey MainWindowResizeGripTexture2 => _mainWindowResizeGripTexture2 ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(MainWindowResizeGripTexture2));

    public static ComponentResourceKey DialogBackground => _dialogBackground ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(DialogBackground));

    public static ComponentResourceKey DialogContentBackground => _dialogContentBackground ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(DialogContentBackground));

    public static ComponentResourceKey DialogContentText => _dialogContentText ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(DialogContentText));


    public static ComponentResourceKey StatusBarDefault => _statusBarDefault ??=
        new ComponentResourceKey(typeof(WindowColors), nameof(StatusBarDefault));
}

public class CommonControlsColors
{
    private static ComponentResourceKey? _buttonBackground;
    private static ComponentResourceKey? _buttonBorder;
    private static ComponentResourceKey? _buttonText;
    private static ComponentResourceKey? _buttonDefaultBackground;
    private static ComponentResourceKey? _buttonDefaultBorder;
    private static ComponentResourceKey? _buttonDefaultText;
    private static ComponentResourceKey? _buttonFocusedBackground;
    private static ComponentResourceKey? _buttonFocusedBorder;
    private static ComponentResourceKey? _buttonFocusedText;
    private static ComponentResourceKey? _buttonHoverBackground;
    private static ComponentResourceKey? _buttonHoverBorder;
    private static ComponentResourceKey? _buttonHoverText;
    private static ComponentResourceKey? _buttonPressedBackground;
    private static ComponentResourceKey? _buttonPressedBorder;
    private static ComponentResourceKey? _buttonPressedText;
    private static ComponentResourceKey? _buttonDisabledBackground;
    private static ComponentResourceKey? _buttonDisabledBorder;
    private static ComponentResourceKey? _buttonDisabledText;

    public static ComponentResourceKey ButtonBackground => _buttonBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonBackground));

    public static ComponentResourceKey ButtonBorder => _buttonBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonBorder));

    public static ComponentResourceKey ButtonText => _buttonText ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonText));

    public static ComponentResourceKey ButtonDefaultBackground => _buttonDefaultBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonDefaultBackground));

    public static ComponentResourceKey ButtonDefaultBorder => _buttonDefaultBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonDefaultBorder));

    public static ComponentResourceKey ButtonDefaultText => _buttonDefaultText ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonDefaultText));

    public static ComponentResourceKey ButtonFocusedBackground => _buttonFocusedBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonFocusedBackground));

    public static ComponentResourceKey ButtonFocusedBorder => _buttonFocusedBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonFocusedBorder));

    public static ComponentResourceKey ButtonFocusedText => _buttonFocusedText ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonFocusedText));

    public static ComponentResourceKey ButtonHoverBackground => _buttonHoverBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonHoverBackground));

    public static ComponentResourceKey ButtonHoverBorder => _buttonHoverBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonHoverBorder));

    public static ComponentResourceKey ButtonHoverText => _buttonHoverText ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonHoverText));

    public static ComponentResourceKey ButtonPressedBackground => _buttonPressedBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonPressedBackground));

    public static ComponentResourceKey ButtonPressedBorder => _buttonPressedBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonPressedBorder));

    public static ComponentResourceKey ButtonPressedText => _buttonPressedText ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonPressedText));

    public static ComponentResourceKey ButtonDisabledBackground => _buttonDisabledBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonDisabledBackground));

    public static ComponentResourceKey ButtonDisabledBorder => _buttonDisabledBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonDisabledBorder));

    public static ComponentResourceKey ButtonDisabledText => _buttonDisabledText ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ButtonDisabledText));
}