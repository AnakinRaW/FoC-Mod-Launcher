using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Themes.Colors;

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

    private static ComponentResourceKey? _checkBoxBackground;
    private static ComponentResourceKey? _checkBoxBorder;
    private static ComponentResourceKey? _checkBoxGlyph;
    private static ComponentResourceKey? _checkBoxText;
    private static ComponentResourceKey? _checkBoxBackgroundHover;
    private static ComponentResourceKey? _checkBoxBorderHover;
    private static ComponentResourceKey? _checkBoxGlyphHover;
    private static ComponentResourceKey? _checkBoxTextHover;
    private static ComponentResourceKey? _checkBoxBackgroundDown;
    private static ComponentResourceKey? _checkBoxBorderDown;
    private static ComponentResourceKey? _checkBoxGlyphDown;
    private static ComponentResourceKey? _checkBoxTextDown;
    private static ComponentResourceKey? _checkBoxBackgroundDisabled;
    private static ComponentResourceKey? _checkBoxBorderDisabled;
    private static ComponentResourceKey? _checkBoxGlyphDisabled;
    private static ComponentResourceKey? _checkBoxTextDisabled;

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

    
    public static ComponentResourceKey CheckBoxBackground => _checkBoxBackground ??=
            new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxBackground));

    public static ComponentResourceKey CheckBoxBorder =>
        _checkBoxBorder ??= new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxBorder));

    public static ComponentResourceKey CheckBoxGlyph =>
        _checkBoxGlyph ??= new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxGlyph));

    public static ComponentResourceKey CheckBoxText =>
        _checkBoxText ??= new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxText));

    public static ComponentResourceKey CheckBoxBackgroundHover => _checkBoxBackgroundHover ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxBackgroundHover));

    public static ComponentResourceKey CheckBoxBorderHover => _checkBoxBorderHover ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxBorderHover));

    public static ComponentResourceKey CheckBoxGlyphHover => _checkBoxGlyphHover ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxGlyphHover));

    public static ComponentResourceKey CheckBoxTextHover => _checkBoxTextHover ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxTextHover));

    public static ComponentResourceKey CheckBoxBackgroundDown => _checkBoxBackgroundDown ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxBackgroundDown));

    public static ComponentResourceKey CheckBoxBorderDown => _checkBoxBorderDown ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxBorderDown));

    public static ComponentResourceKey CheckBoxGlyphDown => _checkBoxGlyphDown ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxGlyphDown));

    public static ComponentResourceKey CheckBoxTextDown => _checkBoxTextDown ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxTextDown));

    public static ComponentResourceKey CheckBoxBackgroundDisabled => _checkBoxBackgroundDisabled ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxBackgroundDisabled));

    public static ComponentResourceKey CheckBoxBorderDisabled => _checkBoxBorderDisabled ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxBorderDisabled));

    public static ComponentResourceKey CheckBoxGlyphDisabled => _checkBoxGlyphDisabled ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxGlyphDisabled));

    public static ComponentResourceKey CheckBoxTextDisabled => _checkBoxTextDisabled ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(CheckBoxTextDisabled));
}