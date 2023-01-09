using System;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

[Flags]
public enum MenuShowOptions
{
    SelectFirstItem = 1,
    ShowMnemonics = 2,
    SupportsTypeAhead = 4,
    PlaceBottom = 8,
    LeftAlign = 16,
    RightAlign = 32
}