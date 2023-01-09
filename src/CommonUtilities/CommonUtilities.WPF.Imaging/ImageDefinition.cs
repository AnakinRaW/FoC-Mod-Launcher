using System;

namespace AnakinRaW.CommonUtilities.Wpf.Imaging;

public struct ImageDefinition
{
    public ImageKey ImakgeKey;
    public Uri Source;
    public ImageFileKind Kind;
    public bool CanTheme;
}