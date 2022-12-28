using System;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging;

public struct ImageDefinition
{
    public ImageKey ImakgeKey;
    public Uri Source;
    public ImageFileKind Kind;
    public bool CanTheme;
}