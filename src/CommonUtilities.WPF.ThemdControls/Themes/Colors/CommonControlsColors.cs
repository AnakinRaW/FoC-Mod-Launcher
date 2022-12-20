﻿using System.Windows;

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

    private static ComponentResourceKey? _treeViewBackground;
    private static ComponentResourceKey? _treeViewBorder;
    private static ComponentResourceKey? _treeViewText;
    private static ComponentResourceKey? _treeViewSelectedText;
    private static ComponentResourceKey? _treeViewInactiveText;
    private static ComponentResourceKey? _treeFocusBorder;
    private static ComponentResourceKey? _treeExpander;
    private static ComponentResourceKey? _treeExpanderHover;
    private static ComponentResourceKey? _treeExpanderSelected;
    private static ComponentResourceKey? _treeExpanderSelectedHover;
    private static ComponentResourceKey? _treeExpanderInactive;
    private static ComponentResourceKey? _treeExpanderInactiveHover;
    private static ComponentResourceKey? _treeItemHover;
    private static ComponentResourceKey? _treeItemSelectedBackground;
    private static ComponentResourceKey? _treeItemInactiveBackground;

    private static ComponentResourceKey? _scrollBarArrowBackground;
    private static ComponentResourceKey? _scrollBarArrowDisabledBackground;
    private static ComponentResourceKey? _scrollBarArrowGlyph;
    private static ComponentResourceKey? _scrollBarArrowGlyphDisabled;
    private static ComponentResourceKey? _scrollBarArrowGlyphMouseOver;
    private static ComponentResourceKey? _scrollBarArrowGlyphPressed;
    private static ComponentResourceKey? _scrollBarArrowMouseOverBackground;
    private static ComponentResourceKey? _scrollBarArrowPressedBackground;
    private static ComponentResourceKey? _scrollBarBackground;
    private static ComponentResourceKey? _scrollBarBorder;
    private static ComponentResourceKey? _scrollBarDisabledBackground;
    private static ComponentResourceKey? _scrollBarThumbBackground;
    private static ComponentResourceKey? _scrollBarThumbBorder;
    private static ComponentResourceKey? _scrollBarThumbDisabled;
    private static ComponentResourceKey? _scrollBarThumbGlyph;
    private static ComponentResourceKey? _scrollBarThumbGlyphMouseOverBorder;
    private static ComponentResourceKey? _scrollBarThumbGlyphPressedBorder;
    private static ComponentResourceKey? _scrollBarThumbMouseOverBackground;
    private static ComponentResourceKey? _scrollBarThumbMouseOverBorder;
    private static ComponentResourceKey? _scrollBarThumbPressedBackground;
    private static ComponentResourceKey? _scrollBarThumbPressedBorder;
    private static ComponentResourceKey? _autoHideResizeGrip;

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


    public static ComponentResourceKey TreeViewBackground => _treeViewBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeViewBackground));

    public static ComponentResourceKey TreeViewBorder => _treeViewBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeViewBorder));

    public static ComponentResourceKey TreeViewText => _treeViewText ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeViewText));

    public static ComponentResourceKey TreeViewSelectedText => _treeViewSelectedText ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeViewSelectedText));

    public static ComponentResourceKey TreeViewInactiveText => _treeViewInactiveText ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeViewInactiveText));

    public static ComponentResourceKey TreeFocusBorder => _treeFocusBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeFocusBorder));

    public static ComponentResourceKey TreeExpander => _treeExpander ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeExpander));

    public static ComponentResourceKey TreeExpanderHover => _treeExpanderHover ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeExpanderHover));

    public static ComponentResourceKey TreeExpanderSelected => _treeExpanderSelected ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeExpanderSelected));

    public static ComponentResourceKey TreeExpanderSelectedHover => _treeExpanderSelectedHover ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeExpanderSelectedHover));

    public static ComponentResourceKey TreeExpanderInactive => _treeExpanderInactive ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeExpanderInactive));

    public static ComponentResourceKey TreeExpanderInactiveHover => _treeExpanderInactiveHover ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeExpanderInactiveHover));

    public static ComponentResourceKey TreeItemHover => _treeItemHover ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeItemHover));

    public static ComponentResourceKey TreeItemSelectedBackground => _treeItemSelectedBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeItemSelectedBackground));

    public static ComponentResourceKey TreeItemInactiveBackground => _treeItemInactiveBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(TreeItemInactiveBackground));

    public static ComponentResourceKey ScrollBarArrowBackground => _scrollBarArrowBackground ??=
           new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarArrowBackground));

    public static ComponentResourceKey ScrollBarArrowDisabledBackground => _scrollBarArrowDisabledBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarArrowDisabledBackground));

    public static ComponentResourceKey ScrollBarArrowGlyph => _scrollBarArrowGlyph ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarArrowGlyph));

    public static ComponentResourceKey ScrollBarArrowGlyphDisabled => _scrollBarArrowGlyphDisabled ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarArrowGlyphDisabled));

    public static ComponentResourceKey ScrollBarArrowGlyphMouseOver => _scrollBarArrowGlyphMouseOver ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarArrowGlyphMouseOver));

    public static ComponentResourceKey ScrollBarArrowGlyphPressed => _scrollBarArrowGlyphPressed ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarArrowGlyphPressed));

    public static ComponentResourceKey ScrollBarArrowMouseOverBackground => _scrollBarArrowMouseOverBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarArrowMouseOverBackground));

    public static ComponentResourceKey ScrollBarArrowPressedBackground => _scrollBarArrowPressedBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarArrowPressedBackground));

    public static ComponentResourceKey ScrollBarBackground => _scrollBarBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarBackground));

    public static ComponentResourceKey ScrollBarBorder => _scrollBarBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarBorder));

    public static ComponentResourceKey ScrollBarDisabledBackground => _scrollBarDisabledBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarDisabledBackground));


    public static ComponentResourceKey ScrollBarThumbBackground => _scrollBarThumbBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarThumbBackground));

    public static ComponentResourceKey ScrollBarThumbBorder => _scrollBarThumbBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarThumbBorder));

    public static ComponentResourceKey ScrollBarThumbDisabled => _scrollBarThumbDisabled ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarThumbDisabled));

    public static ComponentResourceKey ScrollBarThumbGlyph => _scrollBarThumbGlyph ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarThumbGlyph));

    public static ComponentResourceKey ScrollBarThumbGlyphMouseOverBorder => _scrollBarThumbGlyphMouseOverBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarThumbGlyphMouseOverBorder));

    public static ComponentResourceKey ScrollBarThumbGlyphPressedBorder => _scrollBarThumbGlyphPressedBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarThumbGlyphPressedBorder));

    public static ComponentResourceKey ScrollBarThumbMouseOverBackground => _scrollBarThumbMouseOverBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarThumbMouseOverBackground));

    public static ComponentResourceKey ScrollBarThumbMouseOverBorder => _scrollBarThumbMouseOverBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarThumbMouseOverBorder));

    public static ComponentResourceKey ScrollBarThumbPressedBackground => _scrollBarThumbPressedBackground ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarThumbPressedBackground));

    public static ComponentResourceKey ScrollBarThumbPressedBorder => _scrollBarThumbPressedBorder ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(ScrollBarThumbPressedBorder));

    public static ComponentResourceKey AutoHideResizeGrip => _autoHideResizeGrip ??=
        new ComponentResourceKey(typeof(CommonControlsColors), nameof(AutoHideResizeGrip));
}